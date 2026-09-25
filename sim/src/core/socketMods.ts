// SocketMod ports (Assembly-CSharp/SocketMod*.cs). Each mod is built from its dumped fields.
// Boat, road, monument, water and tutorial specifics collapse to their "open field" outcome:
// the simulator has flat terrain, no water, no monuments and no roads.
import type { Placement } from './construction';
import type { SimEntity } from './entity';
import { OBB } from './obb';
import { QueryTriggerInteraction, type Collider, type PhysicsWorld } from './physics';
import type { PlacementContext, SocketBase } from './sockets';
import { Bounds, Quaternion, Ray, Vector3, clamp01 } from './unity';

type J = any;

/** What socket mods need from the simulation. */
export interface ModServer {
  physics: PhysicsWorld;
  visEntities(center: Vector3, radius: number, layerMask?: number, qti?: QueryTriggerInteraction): SimEntity[];
  /** TerrainMeta.SampleTerrainMeshHeight */
  terrainHeight(p: Vector3): number;
  /** AttractionPoint attributes of an entity's prefab. */
  attractionPoints(e: SimEntity): { groupName: string; worldPosition: Vector3; localRotation: Quaternion }[];
  /** Class chain of an entity (for type checks). */
  classesOf(e: SimEntity): string[];
}

export interface SocketMod {
  readonly kind: string;
  readonly hierachyName: string;
  baseSocket: SocketBase | null;
  socketGrouping: SocketMod | null;
  modifyPlacement(placement: Placement, ctx: PlacementContext): void;
  doCheck(placement: Placement, ctx: PlacementContext): boolean;
  errorMessage(): string;
}

const vec = (a: number[] | undefined, d = Vector3.zero) => (a ? new Vector3(a[0], a[1], a[2]) : d);
const quat = (a: number[] | undefined) => (a ? new Quaternion(a[0], a[1], a[2], a[3]) : Quaternion.identity);

const ERR = {
  NotStableEnough: 'Not stable enough',
  MustPlaceOnConstruction: 'Must be placed on construction',
  CantPlaceOnConstruction: 'Cannot be placed on construction',
  InvalidAreaVehicleLarge: 'Too close to a large vehicle',
  InvalidAngle: 'Invalid angle',
  NotInTerrain: 'Not in terrain',
  WantsInside: 'Must be placed inside',
  WantsOutside: 'Must be placed outside',
  InWater: 'Cannot be placed in water',
  WantsWater: 'Must be placed in water',
  InvalidEntity: 'Invalid entity',
  InvalidEntityType: 'Invalid entity type',
};

function server(ctx: PlacementContext): ModServer {
  if (!ctx.server) throw new Error('socket mod needs a server context');
  return ctx.server;
}

/** Placement.ShouldIgnoreEntity / ShouldIgnoreCollider */
const ignores = (p: Placement, e: SimEntity | null) => p.ignoredEntity != null && e != null && e === p.ignoredEntity;

abstract class ModBase implements SocketMod {
  abstract readonly kind: string;
  hierachyName: string;
  baseSocket: SocketBase | null = null;
  socketGrouping: SocketMod | null = null;
  worldPosition: Vector3;
  worldRotation: Quaternion;
  localPosition: Vector3;
  localRotation: Quaternion;
  failedPhrase: string;
  protected lastError = '';

  constructor(protected j: J) {
    this.hierachyName = j.hierachyName ?? '';
    this.worldPosition = vec(j.worldPosition);
    this.worldRotation = quat(j.worldRotation);
    this.localPosition = vec(j.localPosition);
    this.localRotation = quat(j.localRotation);
    this.failedPhrase = j.FailedPhrase?.legacyEnglish || j.FailedPhrase?.token || '';
  }
  modifyPlacement(_p: Placement, _ctx: PlacementContext) {}
  doCheck(_p: Placement, _ctx: PlacementContext): boolean {
    return false;
  }
  protected errorPhrase(): string {
    return this.lastError;
  }
  errorMessage() {
    return this.failedPhrase || this.errorPhrase() || this.kind;
  }
  protected worldPos(p: Placement) {
    return p.position.add(p.rotation.rotate(this.worldPosition));
  }
}

const LARGE_VEHICLE = 0x8000000;

class AreaCheck extends ModBase {
  kind = 'SocketMod_AreaCheck';
  bounds: Bounds;
  layerMask: number;
  wantsInside: boolean;
  ignoreAntiLargeVehicleCheck: boolean;
  constructor(j: J) {
    super(j);
    this.bounds = new Bounds(vec(j.bounds?.center), vec(j.bounds?.size, new Vector3(0.1, 0.1, 0.1)));
    this.layerMask = j.layerMask ?? 0;
    this.wantsInside = j.wantsInside ?? true;
    this.ignoreAntiLargeVehicleCheck = !!j.ignoreAntiLargeVehicleCheck;
  }

  /** SocketMod_AreaCheck.IsInArea */
  static isInArea(srv: ModServer, obb: OBB, layerMask: number, wantsInside: boolean, shouldParent: boolean, parent: SimEntity | null, ignored: SimEntity | null) {
    let cols: Collider[] = srv.physics.overlapOBB(obb, layerMask, QueryTriggerInteraction.UseGlobal);
    if (ignored) cols = cols.filter((c) => c.entity !== ignored);
    let foundParent = false;
    if (shouldParent && wantsInside) foundParent = cols.some((c) => c.entity === parent);
    return { hit: cols.length > 0, foundParent };
  }

  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const pos = this.worldPos(p);
    const rot = p.rotation.mul(this.worldRotation);
    const { hit, foundParent } = AreaCheck.isInArea(srv, OBB.fromBounds(pos, rot, this.bounds), this.layerMask, this.wantsInside, !p.parentPassed && p.shouldParent, p.parentEntity, p.ignoredEntity);
    let flag = hit === this.wantsInside;
    p.parentPassed ||= foundParent;
    if (!flag) {
      this.lastError = ERR.NotStableEnough;
      if (this.layerMask === 2097152 || this.layerMask === 136314880) this.lastError = this.wantsInside ? ERR.MustPlaceOnConstruction : ERR.CantPlaceOnConstruction;
    } else if (!this.ignoreAntiLargeVehicleCheck && this.wantsInside && (this.layerMask & LARGE_VEHICLE) === 0) {
      flag = !srv.physics.checkSphere(p.position, 5, LARGE_VEHICLE);
      if (!flag) this.lastError = ERR.InvalidAreaVehicleLarge;
    }
    return flag;
  }
}

class SphereCheck extends ModBase {
  kind = 'SocketMod_SphereCheck';
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const j = this.j;
    const layerMask: number = j.layerMask ?? 0;
    const wantsCollide = !!j.wantsCollide;
    let cols = srv.physics.overlapSphere(this.worldPos(p), j.sphereRadius ?? 1, layerMask, QueryTriggerInteraction.Collide);
    if (p.ignoredEntity) cols = cols.filter((c) => !ignores(p, c.entity));
    // requireMonument: no monuments in the simulator, every collider is removed.
    if (j.requireMonument) cols = cols.filter((c) => c.hasCustomTag('BlockBarricadePlacement'));
    let flag = wantsCollide === cols.length > 0;
    const whitelist: J[] = j.entityWhitelist ?? [];
    if (whitelist.length) {
      for (const c of cols) if (c.entity) flag = checkEntityList(c.entity, whitelist, true);
    }
    if (!flag) {
      this.lastError = ERR.NotStableEnough;
      if (layerMask === 2097152 || layerMask === 136314880) this.lastError = wantsCollide ? ERR.MustPlaceOnConstruction : ERR.CantPlaceOnConstruction;
    } else if (!j.ignoreAntiLargeVehicleCheck && wantsCollide && (layerMask & LARGE_VEHICLE) === 0) {
      flag = !srv.physics.checkSphere(p.position, 5, LARGE_VEHICLE);
      if (!flag) this.lastError = ERR.InvalidAreaVehicleLarge;
    }
    return flag;
  }
}

class AngleCheck extends ModBase {
  kind = 'SocketMod_AngleCheck';
  protected override errorPhrase() {
    return ERR.InvalidAngle;
  }
  override doCheck(p: Placement): boolean {
    const usePlacementNormal = !!this.j.usePlacementNormal;
    const v = usePlacementNormal ? Vector3.forward : Vector3.up;
    const num = Vector3.dotDegrees(vec(this.j.worldNormal, Vector3.up), p.rotation.rotate(v));
    const within = this.j.withinDegrees ?? 45;
    return usePlacementNormal ? num >= within : num < within;
  }
}

class InWater extends ModBase {
  kind = 'SocketMod_InWater';
  protected override errorPhrase() {
    return this.j.wantsInWater === false ? ERR.InWater : ERR.WantsWater;
  }
  override doCheck(): boolean {
    // No water: isValid == false.
    return (this.j.wantsInWater ?? true) === false;
  }
}

class TerrainCheck extends ModBase {
  kind = 'SocketMod_TerrainCheck';
  static isInTerrain(srv: ModServer, v: Vector3): boolean {
    // Hits on the World layer (rocks, monuments) count as terrain. The simulator's World layer is empty
    // except for anything the user adds; monument tags are not modelled.
    if (srv.physics.raycastAll(new Ray(v.add(new Vector3(0, 3, 0)), Vector3.down), 3, 65536).length > 0) return true;
    return srv.terrainHeight(v) > v.y;
  }
  protected override errorPhrase() {
    return ERR.NotInTerrain;
  }
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    return TerrainCheck.isInTerrain(server(ctx), this.worldPos(p)) === (this.j.wantsInTerrain ?? true);
  }
}

class Grouping extends ModBase {
  kind = 'SocketMod_Grouping';
  members: SocketMod[] = [];
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    if (this.members.length === 0) return true;
    for (const m of this.members) if (m.doCheck(p, ctx)) return true;
    return false;
  }
}

class HotSpot extends ModBase {
  kind = 'SocketMod_HotSpot';
  override modifyPlacement(p: Placement) {
    p.position = this.worldPos(p);
  }
}

function inverseLerp(a: number, b: number, v: number) {
  return a !== b ? clamp01((v - a) / (b - a)) : 0;
}

function slerpQ(a: Quaternion, b: Quaternion, t: number): Quaternion {
  // Quaternion.Lerp (normalized lerp, shortest path)
  const d = Quaternion.dot(a, b);
  const s = d < 0 ? -1 : 1;
  return new Quaternion(a.x + (b.x * s - a.x) * t, a.y + (b.y * s - a.y) * t, a.z + (b.z * s - a.z) * t, a.w + (b.w * s - a.w) * t).normalized();
}

/** QuaternionEx.LookRotationWithOffset */
function lookRotationWithOffset(offset: Vector3, forward: Vector3, _up: Vector3): Quaternion {
  return Quaternion.lookRotation(forward, Vector3.up).mul(Quaternion.lookRotation(offset, Vector3.up).inverse());
}

class Attraction extends ModBase {
  kind = 'SocketMod_Attraction';
  override doCheck(): boolean {
    return true;
  }
  override modifyPlacement(p: Placement, ctx: PlacementContext) {
    const j = this.j;
    if ((j.shiftEnableSnap && !p.isHoldingShift) || ((j.shiftDisableSnap ?? true) && p.isHoldingShift)) return;
    const srv = server(ctx);
    const outer: number = j.outerRadius ?? 1;
    const inner: number = j.innerRadius ?? 0.1;
    let vector = this.worldPos(p);
    const startRotation = p.rotation;
    let bestDist = Number.MAX_VALUE;
    let bestPos = Vector3.zero;
    let bestRot = Quaternion.identity;
    const startPos = p.position;
    for (const item of srv.visEntities(vector, outer * 2)) {
      if (ignores(p, item)) continue;
      for (const ap of srv.attractionPoints(item)) {
        if (ap.groupName !== (j.groupName ?? 'wallbottom')) continue;
        const pt = item.pose.point(ap.worldPosition);
        const mag = pt.sub(vector).magnitude;
        if (j.ignoreRotationForRadiusCheck) {
          const v3 = item.pose.point(ap.worldPosition.withY(0).mul(2));
          const d = Vector3.distance(v3, startPos);
          if (d < bestDist) { bestDist = d; bestPos = v3; bestRot = item.pose.rotation; }
        }
        if (mag > outer) continue;
        const b = j.lockRotation && j.bypassPlayerRotation
          ? item.pose.rotation.mul(ap.localRotation)
          : lookRotationWithOffset(this.worldPosition, pt.sub(p.position), Vector3.up);
        let t = inverseLerp(outer, inner, mag);
        if (j.lockRotation) t = 1;
        if (j.lockRotation) {
          if (j.bypassPlayerRotation) p.rotation = item.pose.rotation.mul(ap.localRotation);
          else {
            const e = p.rotation.eulerAngles;
            const snapped = new Vector3(e.x - (e.x % 90), e.y - (e.y % 90), e.z - (e.z % 90));
            p.rotation = Quaternion.euler(snapped.add(item.pose.rotation.eulerAngles));
          }
        } else p.rotation = slerpQ(p.rotation, b, t);
        vector = this.worldPos(p);
        p.position = p.position.add(pt.sub(vector).mul(t));
      }
    }
    if (bestDist < Number.MAX_VALUE && j.ignoreRotationForRadiusCheck) {
      p.position = bestPos;
      p.rotation = bestRot;
    }
    if (j.applyPostRotationSnapping) {
      let best = Number.MAX_VALUE;
      let r = p.rotation;
      for (const y of [0, 90, 180, 270]) {
        const q = p.rotation.mul(Quaternion.euler(0, y, 0));
        const a = Quaternion.angle(q, startRotation);
        if (a < best) { best = a; r = q; }
      }
      p.rotation = r;
    }
  }
}

const insideDirs = [
  new Vector3(1, 1, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 1), new Vector3(-1, 1, 0),
  new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0.5), new Vector3(-1, 0, 0.5),
].map((v) => v.normalized);

class Inside extends ModBase {
  kind = 'SocketMod_Inside';
  protected override errorPhrase() {
    return this.j.wantsInside === false ? ERR.WantsOutside : ERR.WantsInside;
  }
  static isOutside(srv: ModServer, pos: Vector3, rotation: Quaternion, dirs: Vector3[], layerMask = 136380416) {
    const num = 20;
    let hits = 0;
    let all = true;
    for (const d of dirs) {
      const h = srv.physics.raycast(new Ray(pos, rotation.rotate(d)), num - 0.5, layerMask);
      if (h) {
        if (((layerMask >>> h.collider.layer) & 1) !== 0) hits++;
      } else all = false;
    }
    return all ? hits < 2 : true;
  }
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const bs = this.baseSocket!;
    const v = p.position.add(p.rotation.rotate(bs.localPosition));
    const q = p.rotation.mul(bs.localRotation);
    const pos = v.add(q.rotate(this.localPosition));
    const rot = q.mul(this.localRotation);
    const dirs = this.j.customDirections ? (this.j.customRayDirections ?? []).map((a: number[]) => vec(a)) : insideDirs;
    const outside = Inside.isOutside(server(ctx), pos, rot, dirs);
    return !(this.j.wantsInside ?? true) === outside;
  }
}

class BuildingBlockMod extends ModBase {
  kind = 'SocketMod_BuildingBlock';
  protected override errorPhrase() {
    return ERR.MustPlaceOnConstruction;
  }
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const contained = srv
      .visEntities(this.worldPos(p), this.j.sphereRadius ?? 1, this.j.layerMask ?? 0, this.j.queryTriggers ?? 0)
      .some((e) => srv.classesOf(e).includes('BuildingBlock'));
    const wants = !!this.j.wantsCollide;
    if (!contained || !wants) return contained ? false : !wants;
    return true;
  }
}

class EntityCheck extends ModBase {
  kind = 'SocketMod_EntityCheck';
  protected override errorPhrase() {
    return ERR.InvalidEntity;
  }
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const wants = !!this.j.wantsCollide;
    let result = !wants;
    const types: J[] = this.j.entityTypes ?? [];
    for (const e of srv.visEntities(this.worldPos(p), this.j.sphereRadius ?? 1, this.j.layerMask ?? 0, this.j.queryTriggers ?? 0)) {
      if (ignores(p, e)) continue;
      const match = types.some((t) => entityRefMatches(e, t));
      if (match && wants) return true;
      if (match && !wants) return false;
    }
    return result;
  }
}

class EntityType extends ModBase {
  kind = 'SocketMod_EntityType';
  protected override errorPhrase() {
    return ERR.InvalidEntityType;
  }
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const wants = !!this.j.wantsCollide;
    const searchType: string = this.j.searchType?.$entity ?? this.j.searchType?.$type ?? '';
    for (const e of srv.visEntities(this.worldPos(p), this.j.sphereRadius ?? 1, this.j.layerMask ?? 0, this.j.queryTriggers ?? 0)) {
      if (ignores(p, e)) continue;
      // item.GetType().IsAssignableFrom(searchType.GetType()): searchType derives from the entity's type.
      const match = srv.classesOf(e)[0] === searchType;
      if (match && wants) return true;
      if (match && !wants) return false;
    }
    return !wants;
  }
}

class PlantCheck extends ModBase {
  kind = 'SocketMod_PlantCheck';
  override doCheck(p: Placement, ctx: PlacementContext): boolean {
    const srv = server(ctx);
    const wants = !!this.j.wantsCollide;
    for (const e of srv.visEntities(this.worldPos(p), this.j.sphereRadius ?? 1, this.j.layerMask ?? 0, this.j.queryTriggers ?? 0)) {
      if (ignores(p, e)) continue;
      const growable = srv.classesOf(e).includes('GrowableEntity');
      if (growable && wants) return true;
      if (growable && !wants) return false;
    }
    return !wants;
  }
}

/** Mods whose conditions can't fail in the simulator's world (no boats, roads, water, monuments). */
class AlwaysPass extends ModBase {
  kind: string;
  constructor(j: J) {
    super(j);
    this.kind = j.$type;
  }
  override doCheck(): boolean {
    return true;
  }
}

/** Boat-only mods that require a boat to be present. */
class AlwaysFail extends ModBase {
  kind: string;
  constructor(j: J) {
    super(j);
    this.kind = j.$type;
  }
  override doCheck(): boolean {
    return false;
  }
}

function entityRefMatches(e: SimEntity, ref: J): boolean {
  if (!ref) return false;
  if (Array.isArray(ref.prefabIDs) && ref.prefabIDs.length) return ref.prefabIDs.includes(e.prefab.prefabID);
  if (typeof ref.prefabID === 'number' && ref.prefabID) return ref.prefabID === e.prefab.prefabID;
  return false;
}

/** DeployVolume.CheckEntityList */
export function checkEntityList(e: SimEntity | null, list: J[], trueIfAnyFound: boolean): boolean {
  if (!list || list.length === 0) return true;
  const found = e != null && list.some((r) => entityRefMatches(e, r));
  return trueIfAnyFound ? found : !found;
}

export function createSocketMods(socket: SocketBase, raw: J[]): SocketMod[] {
  const mods: ModBase[] = raw.map((j) => {
    switch (j.$type) {
      case 'SocketMod_AreaCheck': return new AreaCheck(j);
      case 'SocketMod_SphereCheck': return new SphereCheck(j);
      case 'SocketMod_AngleCheck': return new AngleCheck(j);
      case 'SocketMod_InWater': return new InWater(j);
      case 'SocketMod_TerrainCheck': return new TerrainCheck(j);
      case 'SocketMod_Grouping': return new Grouping(j);
      case 'SocketMod_HotSpot': return new HotSpot(j);
      case 'SocketMod_Attraction': return new Attraction(j);
      case 'SocketMod_Inside': return new Inside(j);
      case 'SocketMod_BuildingBlock': return new BuildingBlockMod(j);
      case 'SocketMod_EntityCheck': return new EntityCheck(j);
      case 'SocketMod_EntityType': return new EntityType(j);
      case 'SocketMod_PlantCheck': return new PlantCheck(j);
      // Need a boat nearby / on a boat:
      case 'SocketMod_BoatBuildingBlock':
        return j.wantsCollide ? new AlwaysFail(j) : new AlwaysPass(j);
      case 'SocketMod_BoatBuildingNettingPoint':
      case 'SocketMod_Anchor':
      case 'SocketMod_WaterDepth': // needs water
      case 'SocketMod_PhysicMaterial': // needs e.g. snow ground
        return new AlwaysFail(j);
      // Pass in an open world with no water/roads/boats/monuments:
      default:
        return new AlwaysPass(j);
    }
  });
  for (const m of mods) m.baseSocket = socket;
  // SocketGrouping: a mod whose parent GameObject carries a SocketMod_Grouping.
  for (const g of mods) {
    if (!(g instanceof Grouping)) continue;
    for (const m of mods) {
      if (m === g) continue;
      const parent = m.hierachyName.slice(0, m.hierachyName.lastIndexOf('/'));
      if (parent === g.hierachyName) {
        m.socketGrouping = g;
        g.members.push(m);
      }
    }
  }
  return mods;
}
