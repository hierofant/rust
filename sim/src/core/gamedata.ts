// Loads public/data/game.json (produced by tools/compile-data.ts) into runtime objects.
import type { PrefabDef } from './entity';
import {
  ConstructionSocket,
  ConstructionSocketElevator,
  CornerSocket,
  NeighbourSocket,
  SocketBase,
  SocketFree,
  SocketFreeSnappable,
  SocketSpecific,
  SocketSpecificFemale,
  SocketTerrain,
  StabilitySocket,
} from './sockets';
import { Bounds, Quaternion, Vector3 } from './unity';

type J = any;

export type Vec3Json = [number, number, number];
export type QuatJson = [number, number, number, number];

export interface ColliderJson {
  type: 'Box' | 'Sphere' | 'Capsule' | 'Mesh' | string;
  node: string;
  layer: number;
  tag: string;
  customTags: string[];
  flags: number;
  active: boolean;
  enabled: boolean;
  isTrigger: boolean;
  pos: Vec3Json;
  rot: QuatJson;
  scale: Vec3Json;
  center?: Vec3Json;
  size?: Vec3Json;
  radius?: number;
  height?: number;
  direction?: number;
  convex?: boolean;
  mesh?: string;
}

export interface GradeDef {
  grade: number;
  gradeName: string;
  skin: number;
  baseHealth: number;
  cost: [string, number][];
  skinPrefab: number | null;
}

export interface ConstructionDef {
  /** All serialized Construction fields, cleaned (vectors as arrays). */
  raw: J;
  name: string | null;
  grades: GradeDef[];
  defaultGrade: GradeDef | null;
  deployable: J | null;
  proximities: { worldPosition: Vector3 }[];
  maxplaceDistance: number;
  rotationAmount: Vector3;
  applyStartingRotation: Vector3;
  canBypassBuildingPermission: boolean;
  isBuildingPrivilege: boolean;
  bounds: Bounds;
}

export interface ItemDef {
  itemid: number;
  shortname: string;
  name: string;
  category: string;
  hidden?: boolean;
  dlc?: boolean;
  steamItem?: boolean;
  redirectOf?: string | boolean;
  deploys?: number;
  deploysGuessed?: boolean;
}

export interface LoadedPrefab extends PrefabDef {
  classes: string[];
  layer: number;
  colliders: ColliderJson[];
  volumes: J[];
  conditionals: J[];
  construction: ConstructionDef | null;
  isBuildingBlock: boolean;
}

const VEC_FIELDS = new Set(['worldPosition', 'localPosition', 'selectSize', 'selectCenter', 'idealPlacementNormal']);
const QUAT_FIELDS = new Set(['worldRotation', 'localRotation']);
// Non-serialized fields recomputed at runtime (position/rotation are getters on SocketBase).
const SKIP_FIELDS = new Set(['$type', 'position', 'rotation', 'hierachyName', 'socketMods', 'cachedType']);

const vec = (a: number[] | null | undefined) => (a ? new Vector3(a[0], a[1], a[2]) : Vector3.zero);
const quat = (a: number[] | null | undefined) => (a ? new Quaternion(a[0], a[1], a[2], a[3]) : Quaternion.identity);
const bounds = (b: J) => (b ? new Bounds(vec(b.center), vec(b.size)) : new Bounds(Vector3.zero, Vector3.zero));

const SOCKET_TYPES: Record<string, new () => SocketBase> = {
  ConstructionSocket,
  ConstructionSocket_Elevator: ConstructionSocketElevator,
  NeighbourSocket,
  StabilitySocket,
  CornerSocket,
  Socket_Specific: SocketSpecific,
  Socket_Specific_Female: SocketSpecificFemale,
  Socket_Free: SocketFree,
  Socket_Free_Snappable: SocketFreeSnappable,
  Socket_Terrain: SocketTerrain,
};

export class UnsupportedSocket extends SocketBase {
  kind: string;
  constructor(kind: string) {
    super();
    this.kind = kind;
  }
  override testTarget(): boolean {
    return false;
  }
}

export function createSocket(j: J): SocketBase {
  const Ctor = SOCKET_TYPES[j.$type];
  const s: SocketBase = Ctor ? new Ctor() : new UnsupportedSocket(j.$type);
  const target = s as unknown as Record<string, unknown>;
  for (const [k, v] of Object.entries(j)) {
    if (SKIP_FIELDS.has(k)) continue;
    if (VEC_FIELDS.has(k)) target[k] = vec(v as number[]);
    else if (QUAT_FIELDS.has(k)) target[k] = quat(v as number[]);
    else if (k === 'checkOccupiedSockets') target[k] = v ?? [];
    else target[k] = v;
  }
  // Socket mods are attached by the placement module (socketMods.ts) from j.socketMods.
  (s as unknown as { rawMods: J[] }).rawMods = j.socketMods ?? [];
  return s;
}

export class GameData {
  readonly prefabs = new Map<number, LoadedPrefab>();
  readonly byPath = new Map<string, LoadedPrefab>();
  readonly items: ItemDef[];
  readonly layers: Record<string, string>;
  readonly meshIndex: Record<string, [number, number, number, number]>;
  meshBuffer: ArrayBuffer | null = null;

  constructor(readonly json: J) {
    this.items = json.items;
    this.layers = json.layers;
    this.meshIndex = json.meshes;
    for (const p of json.prefabs) {
      const lp = loadPrefab(p);
      this.prefabs.set(lp.prefabID, lp);
      this.byPath.set(lp.path, lp);
    }
  }

  static async fetch(base = './data/'): Promise<GameData> {
    const [json, bin] = await Promise.all([
      fetch(base + 'game.json').then((r) => r.json()),
      fetch(base + 'meshes.bin').then((r) => r.arrayBuffer()),
    ]);
    const g = new GameData(json);
    g.meshBuffer = bin;
    return g;
  }

  get(path: string): LoadedPrefab {
    const p = this.byPath.get(path) ?? this.byPath.get(`assets/prefabs/${path}.prefab`);
    if (!p) throw new Error(`prefab not found: ${path}`);
    return p;
  }

  mesh(id: string): { vertices: Float32Array; indices: Uint32Array } | null {
    const e = this.meshIndex[id];
    if (!e || !this.meshBuffer) return null;
    const [vOff, vCount, iOff, iCount] = e;
    return {
      vertices: new Float32Array(this.meshBuffer, vOff, vCount * 3),
      indices: new Uint32Array(this.meshBuffer, iOff, iCount),
    };
  }

  /** Placeable constructions (building blocks + deployables). */
  get placeables(): LoadedPrefab[] {
    return [...this.prefabs.values()].filter((p) => p.construction);
  }
}

function loadPrefab(p: J): LoadedPrefab {
  const c = p.construction;
  const sockets: SocketBase[] = c ? c.sockets.map(createSocket) : [];
  const classes: string[] = p.classes ?? [];
  let construction: ConstructionDef | null = null;
  if (c) {
    construction = {
      raw: c,
      name: c.name,
      grades: c.grades,
      defaultGrade: c.grades[0] ?? null,
      deployable: c.deployable,
      proximities: (c.proximities ?? []).map((x: J) => ({ worldPosition: vec(x.worldPosition) })),
      maxplaceDistance: c.maxplaceDistance ?? 4,
      rotationAmount: vec(c.rotationAmount),
      applyStartingRotation: vec(c.applyStartingRotation),
      canBypassBuildingPermission: !!c.canBypassBuildingPermission,
      isBuildingPrivilege: !!c.isBuildingPrivilege,
      bounds: bounds(c.bounds ?? p.bounds),
    };
  }
  return {
    prefabID: p.id,
    path: p.path,
    sockets,
    bounds: bounds(p.bounds),
    isStabilityEntity: classes.includes('StabilityEntity'),
    grounded: !!p.grounded,
    classes,
    layer: p.layer,
    colliders: p.colliders,
    volumes: p.volumes,
    conditionals: p.conditionals,
    construction,
    isBuildingBlock: classes.includes('BuildingBlock'),
  };
}
