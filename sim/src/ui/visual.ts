// Client visuals (meshes + textures from the game client, compiled by tools/compile-visual.ts).
// Optional: without public/visual/ the app falls back to drawing collision shapes.
import * as THREE from 'three';

type J = any;

export interface VisualPart {
  node: string;
  pos: [number, number, number];
  rot: [number, number, number, number];
  scale: [number, number, number];
  mesh: string;
  materials: { shader?: string; texture: string | null; color?: number[]; transparent?: boolean; cutoff?: number | null }[];
}

export class Visuals {
  private geoCache = new Map<string, THREE.BufferGeometry>();
  private matCache = new Map<string, THREE.Material>();
  private texCache = new Map<string, THREE.Texture>();
  private loader = new THREE.TextureLoader();

  constructor(private json: J, private bin: ArrayBuffer, private base: string) {}

  static async load(base: string): Promise<Visuals | null> {
    try {
      const [jr, br] = await Promise.all([fetch(base + 'visual.json'), fetch(base + 'visual.bin')]);
      if (!jr.ok || !br.ok) return null;
      return new Visuals(await jr.json(), await br.arrayBuffer(), base);
    } catch {
      return null;
    }
  }

  parts(prefabPath: string): VisualPart[] | null {
    return this.json.prefabs[prefabPath] ?? null;
  }

  iconUrl(shortname: string) {
    return `${this.base}icons/${shortname}.webp`;
  }

  /** Geometry in three.js space (z flipped, winding reversed), one group per submesh. */
  geometry(id: string): THREE.BufferGeometry | null {
    let g = this.geoCache.get(id);
    if (g) return g;
    const m = this.json.meshes[id];
    if (!m) return null;
    const src = new Float32Array(this.bin, m.vOff, m.vCount * 8);
    const pos = new Float32Array(m.vCount * 3), nor = new Float32Array(m.vCount * 3), uv = new Float32Array(m.vCount * 2);
    for (let i = 0; i < m.vCount; i++) {
      pos[i * 3] = src[i * 8]; pos[i * 3 + 1] = src[i * 8 + 1]; pos[i * 3 + 2] = -src[i * 8 + 2];
      nor[i * 3] = src[i * 8 + 3]; nor[i * 3 + 1] = src[i * 8 + 4]; nor[i * 3 + 2] = -src[i * 8 + 5];
      // Unity UV origin is bottom-left like GL; three flips textures by default, so keep v as is.
      uv[i * 2] = src[i * 8 + 6]; uv[i * 2 + 1] = src[i * 8 + 7];
    }
    const indices: number[] = [];
    g = new THREE.BufferGeometry();
    for (const [off, count] of m.submeshes as [number, number][]) {
      const idx = new Uint32Array(this.bin, off, count);
      const start = indices.length;
      for (let i = 0; i < count; i += 3) indices.push(idx[i], idx[i + 2], idx[i + 1]);
      g.addGroup(start, count, g.groups.length);
    }
    g.setAttribute('position', new THREE.BufferAttribute(pos, 3));
    g.setAttribute('normal', new THREE.BufferAttribute(nor, 3));
    g.setAttribute('uv', new THREE.BufferAttribute(uv, 2));
    g.setIndex(indices);
    g.computeBoundingSphere();
    this.geoCache.set(id, g);
    return g;
  }

  material(m: VisualPart['materials'][number] | undefined, tint?: number): THREE.Material {
    const key = JSON.stringify(m ?? null) + '|' + (tint ?? '');
    let mat = this.matCache.get(key);
    if (mat) return mat;
    const c = m?.color ?? [1, 1, 1, 1];
    const glass = !!m?.transparent && m?.cutoff == null;
    const opts: THREE.MeshLambertMaterialParameters = {
      color: tint ?? new THREE.Color(c[0], c[1], c[2]),
      transparent: glass,
      opacity: glass ? Math.min(0.4, c[3] ?? 0.4) : 1,
      depthWrite: !glass,
      alphaTest: m?.cutoff != null ? m.cutoff : 0,
      side: m?.cutoff != null ? THREE.DoubleSide : THREE.FrontSide,
    };
    if (m?.texture) opts.map = this.texture(m.texture);
    mat = new THREE.MeshLambertMaterial(opts);
    this.matCache.set(key, mat);
    return mat;
  }

  private texture(id: string): THREE.Texture | undefined {
    const file = this.json.textures[id];
    if (!file) return undefined;
    let t = this.texCache.get(id);
    if (!t) {
      t = this.loader.load(`${this.base}tex/${file}`);
      t.colorSpace = THREE.SRGBColorSpace;
      t.wrapS = t.wrapT = THREE.RepeatWrapping;
      t.anisotropy = 4;
      this.texCache.set(id, t);
    }
    return t;
  }
}
