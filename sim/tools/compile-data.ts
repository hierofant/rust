// Compiles the raw server dump (data/<buildid>/) into the compact data the app loads
// (public/data/game.json + public/data/meshes.bin).
//
//   npx tsx tools/compile-data.ts [../data/25454815]
//
// Selection: every prefab with a Construction attribute (everything a player can place),
// plus every prefab those reference through grade skins and server-side conditional models.
import { existsSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { basename, join, resolve } from 'node:path';

const root = resolve(process.argv[2] ?? join(import.meta.dirname, '../../data/25454815'));
const outDir = join(import.meta.dirname, '../public/data');

type J = any;
const readJson = (p: string): J => JSON.parse(readFileSync(p, 'utf8'));

// ---------------------------------------------------------------------------
// Raw prefab index

const prefabDir = join(root, 'prefabs');
const pathById = new Map<number, string>();
const idByName = new Map<string, number[]>();
const rawCache = new Map<number, J>();

for (const f of readdirSync(prefabDir)) {
  const id = Number(f.replace('.json', ''));
  // path is in the first few hundred bytes
  const head = readFileSync(join(prefabDir, f), 'utf8').slice(0, 600);
  const m = /"path":\s*"([^"]*)"/.exec(head);
  if (!m) continue;
  pathById.set(id, m[1]);
  const name = basename(m[1]).replace(/\.prefab$/, '');
  idByName.set(name, [...(idByName.get(name) ?? []), id]);
}

function raw(id: number): J | null {
  if (!rawCache.has(id)) {
    const p = join(prefabDir, `${id}.json`);
    rawCache.set(id, existsSync(p) ? readJson(p) : null);
  }
  return rawCache.get(id);
}

// ---------------------------------------------------------------------------
// Generic cleaning of serializer output

const DROP = new Set([
  '$name', 'prefabAttribute', 'gameManager', 'fullName', 'instanceID', 'worldForward', 'localScale',
  'isServer', 'isClient', 'prefabID', '$alreadyDumpedElsewhereInThisFile', 'baseSocket', 'construction',
  'guideMesh', 'guideMeshMaterial', 'placeEffect', 'info', 'upgradeMenu', 'icon',
]);

function isVec(o: J) {
  if (o == null || typeof o !== 'object' || Array.isArray(o)) return false;
  const k = Object.keys(o);
  return (k.length === 3 && 'x' in o && 'y' in o && 'z' in o) || (k.length === 4 && 'w' in o && 'x' in o);
}

function resolveEntityRef(o: J): J {
  // Refs to BaseEntity components carry only the GameObject name in this dump version.
  const name = o.goName ?? o.name;
  const ids = idByName.get(name) ?? [];
  return { $entity: o.$ref, name, prefabIDs: o.prefabID ? [o.prefabID] : ids };
}

function clean(o: J): J {
  if (o == null) return null;
  if (typeof o === 'number') return Number.isFinite(o) ? o : o;
  if (typeof o !== 'object') return o;
  if (Array.isArray(o)) return o.map(clean);
  if (isVec(o)) return 'w' in o ? [o.x, o.y, o.z, o.w] : [o.x, o.y, o.z];
  if ('$enum' in o) return o.value;
  if ('$cycle' in o) return null;
  if ('$ref' in o) {
    if (o.$ref === 'GameObject') return o.resourceID ? { prefab: o.resourceID, path: o.resourcePath } : null;
    if (o.$ref === 'ItemDefinition') return { item: o.shortname, itemid: o.itemid };
    if (o.$ref === 'Mesh') return { mesh: o.meshId };
    if ('goName' in o) return resolveEntityRef(o);
    return { $ref: o.$ref, name: o.name };
  }
  const out: J = {};
  for (const [k, v] of Object.entries(o)) {
    if (DROP.has(k)) continue;
    out[k] = clean(v);
  }
  return out;
}

// ---------------------------------------------------------------------------
// Transforms (prefab space), Unity conventions

type V3 = [number, number, number];
type Q = [number, number, number, number];
const qmul = (a: Q, b: Q): Q => [
  a[3] * b[0] + a[0] * b[3] + a[1] * b[2] - a[2] * b[1],
  a[3] * b[1] + a[1] * b[3] + a[2] * b[0] - a[0] * b[2],
  a[3] * b[2] + a[2] * b[3] + a[0] * b[1] - a[1] * b[0],
  a[3] * b[3] - a[0] * b[0] - a[1] * b[1] - a[2] * b[2],
];
const qrot = (q: Q, v: V3): V3 => {
  const [x, y, z, w] = q;
  const tx = 2 * (y * v[2] - z * v[1]), ty = 2 * (z * v[0] - x * v[2]), tz = 2 * (x * v[1] - y * v[0]);
  return [v[0] + w * tx + (y * tz - z * ty), v[1] + w * ty + (z * tx - x * tz), v[2] + w * tz + (x * ty - y * tx)];
};
const v3 = (o: J): V3 => [o.x, o.y, o.z];
const q4 = (o: J): Q => [o.x, o.y, o.z, o.w];

interface Xf { pos: V3; rot: Q; scale: V3 }
function compose(parent: Xf, node: J): Xf {
  const lp = v3(node.localPosition), lr = q4(node.localRotation), ls = v3(node.localScale);
  const scaled: V3 = [lp[0] * parent.scale[0], lp[1] * parent.scale[1], lp[2] * parent.scale[2]];
  const p = qrot(parent.rot, scaled);
  return {
    pos: [parent.pos[0] + p[0], parent.pos[1] + p[1], parent.pos[2] + p[2]],
    rot: qmul(parent.rot, lr),
    scale: [parent.scale[0] * ls[0], parent.scale[1] * ls[1], parent.scale[2] * ls[2]],
  };
}

// ---------------------------------------------------------------------------
// Colliders from the GameObject hierarchy

const meshesUsed = new Set<string>();

function collectColliders(rootNode: J) {
  const colliders: J[] = [];
  const identity: Xf = { pos: [0, 0, 0], rot: [0, 0, 0, 1], scale: [1, 1, 1] };
  const walk = (node: J, xf: Xf, path: string, active: boolean) => {
    const comps: J[] = node.components ?? [];
    const info = comps.find((c) => c.$type === 'ColliderInfo');
    for (const c of comps) {
      const t: string = c.$type ?? '';
      if (!t.startsWith('UnityEngine.') || !t.endsWith('Collider')) continue;
      const col: J = {
        type: c.colliderType,
        node: path,
        layer: node.layer.index,
        tag: node.tag,
        customTags: node.customTags ?? [],
        flags: info ? clean(info.flags) : 0,
        active,
        enabled: c.enabled,
        isTrigger: c.isTrigger,
        pos: xf.pos,
        rot: xf.rot,
        scale: xf.scale,
      };
      if (c.center) col.center = v3(c.center);
      if (c.size) col.size = v3(c.size);
      if (c.radius != null) col.radius = c.radius;
      if (c.height != null) col.height = c.height;
      if (c.direction != null) col.direction = typeof c.direction === 'object' ? c.direction.value : c.direction;
      if (c.convex != null) col.convex = c.convex;
      if (c.sharedMesh?.meshId) {
        col.mesh = c.sharedMesh.meshId;
        meshesUsed.add(col.mesh);
      }
      colliders.push(col);
    }
    for (const ch of node.children ?? []) walk(ch, compose(xf, ch), `${path}/${ch.name}`, active && ch.activeSelf !== false);
  };
  // Prefab roots are stored inactive and activated on spawn.
  walk(rootNode, identity, rootNode.name, true);
  return colliders;
}

// ---------------------------------------------------------------------------
// Per-prefab compilation

const KEY_CLASSES = new Set([
  'BuildingBlock', 'StabilityEntity', 'DecayEntity', 'Door', 'SimpleBuildingBlock', 'BuildingPrivlidge',
  'StorageContainer', 'IOEntity', 'SleepingBag', 'Deployable', 'BaseCombatEntity', 'Signage', 'BaseLadder',
]);

const compiled = new Map<number, J>();
const queue: number[] = [];
const enqueue = (id: number | undefined) => {
  if (id && !compiled.has(id) && !queue.includes(id) && pathById.has(id)) queue.push(id);
};

// The serializer expands each object once per file and writes later occurrences as
// {$ref, $alreadyDumpedElsewhereInThisFile}. Index expanded objects so refs can be resolved:
// exactly by instanceID when the ref carries one, otherwise by position (see resolveSocketList).
function indexExpanded(d: J) {
  const byInstance = new Map<number, J>();
  const sockets: J[] = [];
  const walk = (o: J) => {
    if (o == null || typeof o !== 'object') return;
    if (Array.isArray(o)) return o.forEach(walk);
    if (o.$type && o.instanceID) byInstance.set(o.instanceID, o);
    if (o.$type && typeof o.socketName === 'string') sockets.push(o);
    for (const v of Object.values(o)) walk(v);
  };
  walk(d);
  return { byInstance, sockets };
}

function resolveSocketList(list: J[], idx: ReturnType<typeof indexExpanded>): J[] {
  const out = list.map((s) => (s.$ref ? idx.byInstance.get(s.instanceID) ?? null : s));
  if (out.every(Boolean)) return out;
  // Fallback: refs without instanceID get the expanded sockets of the same type that are not
  // yet in the list, in socketName order (matches hierarchy order for building blocks).
  const used = new Set(out.filter(Boolean));
  const spare = idx.sockets.filter((s) => !used.has(s)).sort((a, b) => (a.socketName < b.socketName ? -1 : a.socketName > b.socketName ? 1 : 0));
  return out.map((s, i) => {
    if (s) return s;
    const want = list[i].$ref;
    const k = spare.findIndex((x) => x.$type === want);
    if (k < 0) throw new Error(`unresolved socket ref ${want}`);
    return spare.splice(k, 1)[0];
  });
}

function compilePrefab(id: number): J {
  const d = raw(id)!;
  const pa = d.prefabAttributes ?? {};
  const idx = indexExpanded(d);
  const rootComps: J[] = d.hierarchy?.components ?? [];
  const rootEntity = rootComps.find((c) => c.$type && 'bounds' in c && 'grounded' in c) ?? rootComps.find((c) => c.$type && 'bounds' in c);
  const out: J = {
    id,
    path: d.path,
    classes: (d.rootClassChain ?? []).filter((c: string) => !c.includes('.')),
    bounds: d.entityBounds ? clean(d.entityBounds) : null,
    layer: d.hierarchy?.layer?.index ?? 0,
    colliders: d.hierarchy ? collectColliders(d.hierarchy) : [],
  };
  if (rootEntity && 'grounded' in rootEntity) out.grounded = rootEntity.grounded;

  const cons = pa.Construction?.[0];
  if (cons) {
    const c: J = clean({ ...cons, allSockets: undefined, grades: undefined, defaultGrade: undefined, allProximities: undefined, deployable: undefined, placeholder: undefined, socketHandle: undefined });
    delete c.allSockets; delete c.grades; delete c.defaultGrade; delete c.allProximities; delete c.deployable; delete c.placeholder; delete c.socketHandle;
    c.name = cons.info?.name?.legacyEnglish ?? null;
    const sockets = resolveSocketList(cons.allSockets ?? [], idx);
    c.sockets = sockets.map((s: J) => {
      const cs = clean({ ...s, checkOccupiedSockets: undefined });
      cs.checkOccupiedSockets = (s.checkOccupiedSockets ?? []).map((o: J) => {
        const t = o.Socket?.$ref ? idx.byInstance.get(o.Socket.instanceID) : o.Socket;
        return { socketName: t?.socketName ?? null, femaleDummy: !!o.FemaleDummy };
      });
      return cs;
    });
    // grades[0] is a dedup ref to defaultGrade (expanded there).
    const grades = (cons.grades ?? []).map((g: J) => (g.$ref ? cons.defaultGrade : g)).filter((g: J) => g && g.gradeBase);
    c.grades = grades.map((g: J) => ({
      grade: g.gradeBase.type.value,
      gradeName: g.gradeBase.type.name,
      skin: g.gradeBase.skin,
      baseHealth: g.gradeBase.baseHealth,
      cost: (g.gradeBase.baseCost ?? []).map((a: J) => [a.itemDef?.shortname, a.amount]),
      skinPrefab: g.skinObject?.resourceID || null,
      dlc: g.gradeBase.steamDlc ? true : undefined,
    }));
    for (const g of c.grades) enqueue(g.skinPrefab);
    const prox = cons.allProximities ?? [];
    c.proximities = (prox.some((p: J) => p.$ref) ? pa.BuildingProximity ?? [] : prox).map(clean).filter((p: J) => p && !p.$ref);
    const dep = pa.Deployable?.[0] ?? cons.deployable;
    c.deployable = dep && !dep.$ref ? clean(dep) : null;
    const ph = pa.ConstructionPlaceholder?.[0] ?? cons.placeholder;
    c.placeholder = ph && !ph.$ref ? clean(ph) : null;
    out.construction = c;
  }
  out.volumes = (pa.DeployVolume ?? []).map(clean).filter((v: J) => v && !v.$ref);
  out.conditionals = (pa.ConditionalModel ?? [])
    .filter((m: J) => !m.$ref)
    .map((m: J) => {
      const cm = clean(m);
      if (cm.prefab?.prefab && m.onServer) enqueue(cm.prefab.prefab);
      return cm;
    });
  return out;
}

for (const [id] of pathById) {
  const d = raw(id);
  if (d?.prefabAttributes?.Construction) enqueue(id);
  rawCache.delete(id);
}
const placeableCount = queue.length;
while (queue.length) {
  const id = queue.shift()!;
  compiled.set(id, compilePrefab(id));
  rawCache.delete(id);
}

// ---------------------------------------------------------------------------
// Items

const items = readJson(join(root, 'items.json')) as J[];
const constructionByName = new Map<string, number>();
for (const p of compiled.values()) if (p.construction) constructionByName.set(basename(p.path).replace(/\.prefab$/, ''), p.id);

const itemsOut = items.map((it) => {
  const dep = it.itemModDeployable?.entityPrefab?.resourceID as number | undefined;
  const guess = dep ?? constructionByName.get(it.shortname);
  return {
    itemid: it.itemid,
    shortname: it.shortname,
    name: it.displayName?.legacyEnglish ?? it.shortname,
    category: it.category?.name,
    hidden: it.hidden || undefined,
    dlc: it.steamDlc ? true : undefined,
    steamItem: it.steamItem ? true : undefined,
    redirectOf: it.isRedirectOf ? it.isRedirectOf.shortname ?? true : undefined,
    deploys: guess && compiled.has(guess) ? guess : undefined,
    deploysGuessed: dep ? undefined : guess ? true : undefined,
  };
});

// ---------------------------------------------------------------------------
// Meshes -> one binary blob

const meshIndex: Record<string, [number, number, number, number]> = {};
const chunks: Buffer[] = [];
let offset = 0;
const missingMeshes: string[] = [];
for (const meshId of [...meshesUsed].sort()) {
  const p = join(root, 'meshes', `${meshId}.obj`);
  if (!existsSync(p)) { missingMeshes.push(meshId); continue; }
  const verts: number[] = [], idx: number[] = [];
  for (const line of readFileSync(p, 'utf8').split('\n')) {
    if (line.startsWith('v ')) {
      const [, x, y, z] = line.split(/\s+/);
      verts.push(+x, +y, +z);
    } else if (line.startsWith('f ')) {
      const parts = line.trim().split(/\s+/).slice(1).map((s) => parseInt(s.split('/')[0], 10) - 1);
      for (let i = 1; i + 1 < parts.length; i++) idx.push(parts[0], parts[i], parts[i + 1]);
    }
  }
  const vb = Buffer.from(new Float32Array(verts).buffer);
  const ib = Buffer.from(new Uint32Array(idx).buffer);
  meshIndex[meshId] = [offset, verts.length / 3, offset + vb.length, idx.length];
  chunks.push(vb, ib);
  offset += vb.length + ib.length;
}

const layers = readJson(join(root, 'layers.json'));
const convars = readJson(join(root, 'convars.json'));
const player = readJson(join(root, 'player.json'));
const manifest = readJson(join(root, 'manifest.json'));

const game = {
  buildid: manifest.buildid,
  layers: layers.names,
  collisionMatrix: layers.collisionMatrix,
  convars,
  player,
  prefabs: [...compiled.values()],
  items: itemsOut,
  meshes: meshIndex,
};
writeFileSync(join(outDir, 'game.json'), JSON.stringify(game));
writeFileSync(join(outDir, 'meshes.bin'), Buffer.concat(chunks));
console.log(
  `placeable ${placeableCount}, total prefabs ${compiled.size}, items ${itemsOut.length} (deploy mapped ${itemsOut.filter((i) => i.deploys).length}), ` +
    `meshes ${Object.keys(meshIndex).length} (${(offset / 1e6).toFixed(1)} MB), missing ${missingMeshes.length}`,
);
