// Compiles client visuals (data/<buildid>/visual/, see extraction/TASK.md stage 6) into what the
// app loads: public/visual/visual.json + visual.bin + tex/*.webp + icons/*.webp.
//
//   npx tsx tools/compile-visual.ts [../data/25454815/visual]
//
// Meshes are re-indexed from OBJ (separate v/vt/vn indices) into flat vertex buffers:
// per vertex position (3 f32), normal (3 f32), uv (2 f32); indices u32 per submesh.
import { copyFileSync, existsSync, mkdirSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

type J = any;
const src = resolve(process.argv[2] ?? join(import.meta.dirname, '../../data/25454815/visual'));
const out = join(import.meta.dirname, '../public/visual');
const game = JSON.parse(readFileSync(join(import.meta.dirname, '../public/data/game.json'), 'utf8'));
const wanted = new Set<string>(game.prefabs.map((p: J) => p.path));

if (!existsSync(join(src, 'prefabs.json'))) {
  console.error(`no ${join(src, 'prefabs.json')}`);
  process.exit(1);
}
const prefabs: Record<string, J[]> = JSON.parse(readFileSync(join(src, 'prefabs.json'), 'utf8'));

rmSync(out, { recursive: true, force: true });
mkdirSync(join(out, 'tex'), { recursive: true });
mkdirSync(join(out, 'icons'), { recursive: true });

interface MeshOut { vOff: number; vCount: number; submeshes: [number, number][] }
const meshIndex: Record<string, MeshOut> = {};
const chunks: Buffer[] = [];
let offset = 0;
const texUsed = new Set<string>();

function parseObj(text: string) {
  const v: number[] = [], vt: number[] = [], vn: number[] = [];
  const key = new Map<string, number>();
  const pos: number[] = [], nor: number[] = [], uv: number[] = [];
  const groups: number[][] = [];
  let cur: number[] | null = null;
  for (const raw of text.split('\n')) {
    const line = raw.trim();
    if (line.startsWith('v ')) { const p = line.split(/\s+/); v.push(+p[1], +p[2], +p[3]); }
    else if (line.startsWith('vt ')) { const p = line.split(/\s+/); vt.push(+p[1], +p[2]); }
    else if (line.startsWith('vn ')) { const p = line.split(/\s+/); vn.push(+p[1], +p[2], +p[3]); }
    else if (line.startsWith('g ') || line.startsWith('usemtl ')) { cur = []; groups.push(cur); }
    else if (line.startsWith('f ')) {
      if (!cur) { cur = []; groups.push(cur); }
      const corners = line.split(/\s+/).slice(1).map((c) => {
        let k = key.get(c);
        if (k === undefined) {
          const [a, b, n] = c.split('/').map((x) => (x ? parseInt(x, 10) - 1 : -1));
          k = pos.length / 3;
          key.set(c, k);
          pos.push(v[a * 3], v[a * 3 + 1], v[a * 3 + 2]);
          uv.push(b >= 0 ? vt[b * 2] : 0, b >= 0 ? vt[b * 2 + 1] : 0);
          nor.push(n >= 0 ? vn[n * 3] : 0, n >= 0 ? vn[n * 3 + 1] : 1, n >= 0 ? vn[n * 3 + 2] : 0);
        }
        return k;
      });
      for (let i = 1; i + 1 < corners.length; i++) cur.push(corners[0], corners[i], corners[i + 1]);
    }
  }
  return { pos, nor, uv, groups: groups.filter((g) => g.length) };
}

function addMesh(id: string) {
  if (meshIndex[id]) return true;
  const p = join(src, 'meshes', `${id}.obj`);
  if (!existsSync(p)) return false;
  const m = parseObj(readFileSync(p, 'utf8'));
  const inter = new Float32Array((m.pos.length / 3) * 8);
  for (let i = 0; i < m.pos.length / 3; i++) {
    inter.set([m.pos[i * 3], m.pos[i * 3 + 1], m.pos[i * 3 + 2], m.nor[i * 3], m.nor[i * 3 + 1], m.nor[i * 3 + 2], m.uv[i * 2], m.uv[i * 2 + 1]], i * 8);
  }
  const vb = Buffer.from(inter.buffer);
  const entry: MeshOut = { vOff: offset, vCount: m.pos.length / 3, submeshes: [] };
  chunks.push(vb);
  offset += vb.length;
  for (const g of m.groups) {
    const ib = Buffer.from(new Uint32Array(g).buffer);
    entry.submeshes.push([offset, g.length]);
    chunks.push(ib);
    offset += ib.length;
  }
  meshIndex[id] = entry;
  return true;
}

const prefabOut: Record<string, J[]> = {};
let withVisual = 0, missingMesh = 0;
for (const [path, parts] of Object.entries(prefabs)) {
  if (!wanted.has(path) || !Array.isArray(parts)) continue;
  const kept: J[] = [];
  for (const r of parts) {
    if (!r.mesh || !addMesh(r.mesh)) { missingMesh++; continue; }
    for (const m of r.materials ?? []) if (m.texture) texUsed.add(m.texture);
    kept.push({ node: r.node, pos: r.pos, rot: r.rot, scale: r.scale, mesh: r.mesh, materials: r.materials ?? [] });
  }
  if (kept.length) { prefabOut[path] = kept; withVisual++; }
}

let texMissing = 0;
for (const t of texUsed) {
  const s = ['webp', 'png', 'jpg'].map((e) => join(src, 'textures', `${t}.${e}`)).find(existsSync);
  if (s) copyFileSync(s, join(out, 'tex', `${t}.${s.split('.').pop()}`));
  else texMissing++;
}
const texExt: Record<string, string> = {};
for (const f of readdirSync(join(out, 'tex'))) texExt[f.replace(/\.[^.]+$/, '')] = f;

let icons = 0;
if (existsSync(join(src, 'icons'))) {
  for (const f of readdirSync(join(src, 'icons'))) { copyFileSync(join(src, 'icons', f), join(out, 'icons', f)); icons++; }
}

writeFileSync(join(out, 'visual.json'), JSON.stringify({ prefabs: prefabOut, meshes: meshIndex, textures: texExt }));
writeFileSync(join(out, 'visual.bin'), Buffer.concat(chunks));
console.log(`prefabs with visuals ${withVisual}/${wanted.size}, meshes ${Object.keys(meshIndex).length} (${(offset / 1e6).toFixed(1)} MB), ` +
  `textures ${Object.keys(texExt).length} (missing ${texMissing}), icons ${icons}, parts without mesh ${missingMesh}`);
