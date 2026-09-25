// three.js view of the simulation. Entities are drawn from their collision shapes (the server
// dump has no render meshes for most prefabs). Unity is left-handed (+Z forward), three.js is
// right-handed: positions map (x, y, z) -> (x, y, -z), rotations (x, y, z, w) -> (-x, -y, z, w).
import * as THREE from 'three';
import type { SimEntity } from '../core/entity';
import type { LoadedPrefab } from '../core/gamedata';
import { OBB } from '../core/obb';
import type { Collider, Shape } from '../core/physics';
import { colliderShape } from '../core/physics';
import type { BuildServer } from '../core/server';
import { Layer } from '../core/server';
import { Pose, Quaternion, Vector3 } from '../core/unity';

export const toThree = (v: Vector3) => new THREE.Vector3(v.x, v.y, -v.z);
export const fromThree = (v: THREE.Vector3) => new Vector3(v.x, v.y, -v.z);
export const quatToThree = (q: Quaternion) => new THREE.Quaternion(-q.x, -q.y, q.z, q.w);

export const GRADE_COLORS = [0xc8a46a, 0x8a5a2e, 0x9a9a96, 0x8c5a44, 0x3d4247];
const DEPLOYED_COLOR = 0xb07a3c;
const OTHER_COLOR = 0x6f8a9a;

export type ViewMode = 'normal' | 'layers';

function shapeGeometry(shape: Shape): { geo: THREE.BufferGeometry; pos: THREE.Vector3; rot: THREE.Quaternion } | null {
  switch (shape.kind) {
    case 'box': {
      const e = shape.obb.extents;
      return { geo: new THREE.BoxGeometry(e.x * 2, e.y * 2, e.z * 2), pos: toThree(shape.obb.position), rot: quatToThree(shape.obb.rotation) };
    }
    case 'sphere':
      return { geo: new THREE.SphereGeometry(shape.r, 16, 10), pos: toThree(shape.c), rot: new THREE.Quaternion() };
    case 'capsule': {
      const len = Vector3.distance(shape.a, shape.b);
      const geo = new THREE.CapsuleGeometry(shape.r, len, 4, 12);
      const mid = toThree(shape.a.add(shape.b).mul(0.5));
      const dir = toThree(shape.b.sub(shape.a)).normalize();
      const rot = len > 1e-6 ? new THREE.Quaternion().setFromUnitVectors(new THREE.Vector3(0, 1, 0), dir) : new THREE.Quaternion();
      return { geo, pos: mid, rot };
    }
    case 'mesh': {
      const t = shape.tris;
      const pos = new Float32Array(t.length);
      // flip z and reverse winding (b <-> c) to keep faces outward in a right-handed frame
      for (let i = 0; i < t.length; i += 9) {
        const put = (dst: number, src: number) => {
          pos[dst] = t[src];
          pos[dst + 1] = t[src + 1];
          pos[dst + 2] = -t[src + 2];
        };
        put(i, i);
        put(i + 3, i + 6);
        put(i + 6, i + 3);
      }
      const geo = new THREE.BufferGeometry();
      geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
      geo.computeVertexNormals();
      return { geo, pos: new THREE.Vector3(), rot: new THREE.Quaternion() };
    }
    default:
      return null;
  }
}

function colliderColor(srv: BuildServer, c: Collider, e: SimEntity | null): number {
  if (c.layer === Layer.Construction && e && srv.prefab(e).isBuildingBlock) return GRADE_COLORS[e.grade] ?? OTHER_COLOR;
  if (c.layer === Layer.Deployed || c.layer === Layer.Construction) return DEPLOYED_COLOR;
  return OTHER_COLOR;
}

/** Which colliders are drawn in normal view: solid geometry only. */
function visibleInNormal(c: Collider) {
  return !c.isTrigger && c.layer !== Layer.PreventBuilding && c.layer !== Layer.PlayerServer && c.layer !== Layer.Terrain && c.layer !== Layer.ConstructionSocket;
}

const LAYER_VIEW_COLORS: Record<number, number> = {
  [Layer.PreventBuilding]: 0xff3b30, // tier 2: what other objects' deploy volumes test against
  [Layer.Deployed]: 0x34c759,
  [Layer.Construction]: 0x0a84ff,
  [Layer.Trigger]: 0xffd60a,
};

export class Renderer {
  readonly scene = new THREE.Scene();
  readonly camera: THREE.PerspectiveCamera;
  readonly gl: THREE.WebGLRenderer;
  private groups = new Map<SimEntity, { group: THREE.Group; key: string }>();
  private ghost = new THREE.Group();
  private volumeGhost = new THREE.Group();
  private highlight = new THREE.Group();
  mode: ViewMode = 'normal';
  private materials = new Map<string, THREE.Material>();

  constructor(canvas: HTMLCanvasElement, private srv: BuildServer) {
    this.gl = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
    this.gl.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.camera = new THREE.PerspectiveCamera(75, 1, 0.05, 500);
    this.scene.background = new THREE.Color(0x9ec7e8);
    this.scene.fog = new THREE.Fog(0x9ec7e8, 60, 220);
    this.scene.add(new THREE.HemisphereLight(0xdfefff, 0x5a4a3a, 1.3));
    const sun = new THREE.DirectionalLight(0xffffff, 1.6);
    sun.position.set(30, 60, 20);
    this.scene.add(sun);
    const ground = new THREE.Mesh(new THREE.PlaneGeometry(600, 600), new THREE.MeshLambertMaterial({ color: 0x6b7a4a }));
    ground.rotation.x = -Math.PI / 2;
    this.scene.add(ground);
    const grid = new THREE.GridHelper(600, 200, 0x55633a, 0x5d6b40);
    grid.position.y = 0.002;
    this.scene.add(grid);
    this.scene.add(this.ghost, this.volumeGhost, this.highlight);
  }

  resize(w: number, h: number) {
    this.gl.setSize(w, h, false);
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
  }

  private mat(color: number, opts: { opacity?: number; wire?: boolean } = {}): THREE.Material {
    const key = `${color}-${opts.opacity ?? 1}-${opts.wire ? 1 : 0}`;
    let m = this.materials.get(key);
    if (!m) {
      m = opts.wire
        ? new THREE.MeshBasicMaterial({ color, wireframe: true, transparent: true, opacity: opts.opacity ?? 1 })
        : new THREE.MeshLambertMaterial({ color, transparent: (opts.opacity ?? 1) < 1, opacity: opts.opacity ?? 1, depthWrite: (opts.opacity ?? 1) >= 1 });
      this.materials.set(key, m);
    }
    return m;
  }

  private entityKey(e: SimEntity) {
    const cols = this.srv.physics.entityColliders(e);
    return `${this.mode}|${e.grade}|${e.skinPrefab}|${e.modelState.map(Number).join('')}|${cols.length}|${cols.map((c) => c.id).join(',')}`;
  }

  /** Rebuild meshes of entities whose colliders changed. */
  sync() {
    const alive = new Set(this.srv.entities);
    for (const [e, g] of this.groups) {
      if (!alive.has(e)) {
        this.scene.remove(g.group);
        g.group.traverse((o) => (o as THREE.Mesh).geometry?.dispose());
        this.groups.delete(e);
      }
    }
    for (const e of this.srv.entities) {
      const key = this.entityKey(e);
      const cur = this.groups.get(e);
      if (cur && cur.key === key) continue;
      if (cur) {
        this.scene.remove(cur.group);
        cur.group.traverse((o) => (o as THREE.Mesh).geometry?.dispose());
      }
      const group = new THREE.Group();
      group.userData.entity = e;
      for (const c of this.srv.physics.entityColliders(e)) {
        const layerView = this.mode === 'layers';
        if (!layerView && !visibleInNormal(c)) continue;
        const g = shapeGeometry(c.shape);
        if (!g) continue;
        let m: THREE.Material;
        if (layerView) {
          const col = LAYER_VIEW_COLORS[c.layer] ?? OTHER_COLOR;
          m = this.mat(col, { opacity: c.layer === Layer.PreventBuilding ? 0.45 : 0.3 });
        } else m = this.mat(colliderColor(this.srv, c, e));
        const mesh = new THREE.Mesh(g.geo, m);
        mesh.position.copy(g.pos);
        mesh.quaternion.copy(g.rot);
        group.add(mesh);
        if (!layerView) {
          const edges = new THREE.LineSegments(new THREE.EdgesGeometry(g.geo, 25), new THREE.LineBasicMaterial({ color: 0x000000, transparent: true, opacity: 0.25 }));
          edges.position.copy(g.pos);
          edges.quaternion.copy(g.rot);
          group.add(edges);
        }
      }
      this.scene.add(group);
      this.groups.set(e, { group, key });
    }
  }

  private ghostKey = '';

  /** Placement preview: prefab + default skin, built once in prefab space and moved per frame. */
  setGhost(prefab: LoadedPrefab | null, pose: Pose | null, ok: boolean, showVolumes: boolean) {
    if (!prefab || !pose) {
      this.ghost.visible = false;
      this.volumeGhost.visible = false;
      return;
    }
    const key = `${prefab.prefabID}|${ok}|${showVolumes}`;
    if (key !== this.ghostKey) {
      this.ghostKey = key;
      this.buildGhost(prefab, ok, showVolumes);
    }
    for (const g of [this.ghost, this.volumeGhost]) {
      g.visible = true;
      g.position.copy(toThree(pose.position));
      g.quaternion.copy(quatToThree(pose.rotation));
    }
  }

  private buildGhost(prefab: LoadedPrefab, ok: boolean, showVolumes: boolean) {
    this.clear(this.ghost);
    this.clear(this.volumeGhost);
    const color = ok ? 0x30d158 : 0xff453a;
    const origin = new Pose();
    const addShapes = (p: LoadedPrefab, at: Pose) => {
      for (const c of p.colliders) {
        if (!c.active || !c.enabled || c.isTrigger || c.layer === Layer.PreventBuilding) continue;
        const s = colliderShape(c, at, (id) => this.srv.data.mesh(id));
        const g = s && shapeGeometry(s);
        if (!g) continue;
        const mesh = new THREE.Mesh(g.geo, this.mat(color, { opacity: 0.45 }));
        mesh.position.copy(g.pos);
        mesh.quaternion.copy(g.rot);
        this.ghost.add(mesh);
      }
    };
    addShapes(prefab, origin);
    const skin = prefab.construction?.defaultGrade?.skinPrefab;
    const sp = skin != null ? this.srv.data.prefabs.get(skin) : undefined;
    if (sp) {
      addShapes(sp, origin);
      // unconditional server models (e.g. the full wall) so the ghost has a body
      for (const cm of sp.conditionals) {
        if (!cm.onServer || !cm.prefab?.prefab) continue;
        const types = (cm.conditions ?? []).map((c: { $type: string }) => c.$type);
        if (!(types.length === 1 && types[0] === 'ModelConditionTest_Wall') && types.length !== 0) continue;
        const cp = this.srv.data.prefabs.get(cm.prefab.prefab);
        if (cp) addShapes(cp, new Pose(Vector3.from(cm.worldPosition), Quaternion.from(cm.worldRotation)));
      }
    }
    if (!showVolumes) return;
    for (const v of this.srv.volumes(prefab)) {
      const j = v.j;
      let s: Shape | null = null;
      if (j.bounds && (v.kind === 'DeployVolumeOBB' || v.kind.startsWith('DeployVolumeEntityBounds'))) {
        const center = Vector3.from(j.bounds.center);
        const obb = v.kind === 'DeployVolumeOBB'
          ? makeObb(v.worldRotation.rotate(center).add(v.worldPosition), Vector3.from(j.bounds.size), v.worldRotation)
          : makeObb(center, Vector3.from(j.bounds.size), Quaternion.identity);
        s = { kind: 'box', obb };
      } else if (v.kind === 'DeployVolumeSphere') {
        s = { kind: 'sphere', c: v.worldRotation.rotate(Vector3.from(j.center)).add(v.worldPosition), r: j.radius };
      } else if (v.kind === 'DeployVolumeCapsule') {
        const c = v.worldRotation.rotate(Vector3.from(j.center)).add(v.worldPosition);
        const up = v.worldRotation.rotate(Vector3.up).mul(j.height * 0.5);
        s = { kind: 'capsule', a: c.add(up), b: c.sub(up), r: j.radius };
      }
      const g = s && shapeGeometry(s);
      if (!g) continue;
      const mesh = new THREE.Mesh(g.geo, this.mat(0xffd60a, { wire: true, opacity: 0.8 }));
      mesh.position.copy(g.pos);
      mesh.quaternion.copy(g.rot);
      this.volumeGhost.add(mesh);
    }
  }

  private highlighted: SimEntity | null = null;

  setHighlight(e: SimEntity | null) {
    if (e === this.highlighted && this.highlight.children.length) return;
    this.highlighted = e;
    this.clear(this.highlight);
    if (!e) return;
    for (const c of this.srv.physics.entityColliders(e)) {
      if (!visibleInNormal(c)) continue;
      const g = shapeGeometry(c.shape);
      if (!g) continue;
      const lines = new THREE.LineSegments(new THREE.EdgesGeometry(g.geo, 25), new THREE.LineBasicMaterial({ color: 0xffffff }));
      lines.position.copy(g.pos);
      lines.quaternion.copy(g.rot);
      this.highlight.add(lines);
    }
  }

  private clear(g: THREE.Group) {
    for (const c of [...g.children]) {
      g.remove(c);
      (c as THREE.Mesh).geometry?.dispose();
    }
  }

  render() {
    this.gl.render(this.scene, this.camera);
  }
}

function makeObb(pos: Vector3, size: Vector3, rot: Quaternion) {
  return OBB.fromSize(pos, new Vector3(Math.abs(size.x), Math.abs(size.y), Math.abs(size.z)), rot);
}
