// BuildServer: the server-side building simulation on top of World (links + stability).
// Ports Construction.UpdatePlacement, BuildingProximity.Check, Planner.DoBuild and the
// BuildingBlock skin / grade / rotation paths.
import { runTests, type ConditionHost } from './conditions';
import type { Placement, Target } from './construction';
import type { SimEntity } from './entity';
import type { ColliderJson, GameData, GradeDef, LoadedPrefab } from './gamedata';
import { OBB } from './obb';
import { Collider, QueryTriggerInteraction, colliderShape } from './physics';
import type { ModServer } from './socketMods';
import { ConstructionSocket, ConstructionSocketType, NeighbourSocket, StabilitySocket, socketPhysics, type PlacementContext, type SocketBase } from './sockets';
import { Pose, Quaternion, Ray, Vector3 } from './unity';
import { DeployVolume, checkVolumes, type VolumeServer } from './volumes';
import { ObjectWorkQueue } from './workqueue';
import { World, type StabilityConfig } from './world';

type J = any;

export const Layer = {
  Deployed: 8,
  PlayerServer: 17,
  Trigger: 18,
  World: 16,
  Construction: 21,
  ConstructionSocket: 22,
  Terrain: 23,
  PreventBuilding: 29,
} as const;

export interface PlayerState {
  /** transform.position (feet) */
  position: Vector3;
  /** eyes.position */
  eyes: Vector3;
}

export interface PlacementResult {
  ok: boolean;
  position: Vector3;
  rotation: Quaternion;
  error: string;
  /** Male socket that produced the pose. */
  socket: SocketBase | null;
}

interface EntityColliders {
  base: Collider[];
  skin: Collider[];
  conditional: Collider[];
}

export class BuildServer extends World implements ModServer, VolumeServer, ConditionHost {
  readonly skinQueue: ObjectWorkQueue<SimEntity>;
  player: PlayerState = { position: new Vector3(0, 0, -6), eyes: new Vector3(0, 1.5, -6) };
  private playerCollider: Collider | null = null;
  private cols = new Map<SimEntity, EntityColliders>();
  private volumeCache = new Map<number, DeployVolume[]>();
  private conditionalByIid = new Map<number, J>();

  constructor(readonly data: GameData, config: Partial<StabilityConfig> = {}) {
    super(config);
    this.physics.add(new Collider({ kind: 'terrain', height: 0 }, Layer.Terrain, null, { name: 'terrain' }));
    this.skinQueue = new ObjectWorkQueue<SimEntity>((e) => !e.destroyed && this.changeSkin(e));
    socketPhysics.checkOBB = (obb, mask) => this.physics.checkOBB(obb, mask);
    for (const p of data.prefabs.values()) for (const c of p.conditionals) if (c.instanceID) this.conditionalByIid.set(c.instanceID, c);
    this.setPlayer(this.player);
  }

  // ---- ModServer / VolumeServer / ConditionHost ----------------------------

  terrainHeight(_p: Vector3) {
    return 0;
  }

  classesOf(e: SimEntity): string[] {
    return (e.prefab as LoadedPrefab).classes ?? [];
  }

  attractionPoints(e: SimEntity) {
    return ((e.prefab as LoadedPrefab).attractionPoints ?? []).map((a: J) => ({
      groupName: a.groupName as string,
      worldPosition: Vector3.from(a.worldPosition),
      localRotation: Quaternion.from(a.localRotation),
    }));
  }

  volumesOf(e: SimEntity): DeployVolume[] {
    return this.volumes(e.prefab as LoadedPrefab);
  }

  volumes(p: LoadedPrefab): DeployVolume[] {
    let v = this.volumeCache.get(p.prefabID);
    if (!v) {
      v = p.volumes.map((j) => new DeployVolume(j, p.isBuildingBlock));
      this.volumeCache.set(p.prefabID, v);
    }
    return v;
  }

  prefab(e: SimEntity) {
    return e.prefab as LoadedPrefab;
  }

  // ---- player --------------------------------------------------------------

  setPlayer(p: PlayerState) {
    this.player = p;
    if (this.playerCollider) this.physics.remove(this.playerCollider);
    // BasePlayer capsule: radius 0.5, height 1.8, centre 0.9 above the feet.
    const a = p.position.add(new Vector3(0, 0.5, 0));
    const b = p.position.add(new Vector3(0, 1.3, 0));
    this.playerCollider = this.physics.add(new Collider({ kind: 'capsule', a, b, r: 0.5 }, Layer.PlayerServer, null, { name: 'player' }));
  }

  // ---- colliders & skins ---------------------------------------------------

  private addPrefabColliders(e: SimEntity, prefab: LoadedPrefab, pose: Pose, skipPlaceholder = false): Collider[] {
    const out: Collider[] = [];
    const rootName = prefab.colliders[0]?.node.split('/')[0];
    for (const c of prefab.colliders as ColliderJson[]) {
      if (!c.active || !c.enabled) continue;
      // BuildingBlock.placeholderCollider: the root MeshCollider, disabled once a skin exists.
      if (skipPlaceholder && c.type === 'Mesh' && c.node === rootName) continue;
      const shape = colliderShape(c, pose, (id) => this.data.mesh(id));
      if (!shape) continue;
      const col = new Collider(shape, c.layer, e, {
        isTrigger: c.isTrigger,
        flags: c.flags ?? undefined,
        tag: c.tag,
        customTags: c.customTags,
        name: c.node,
      });
      this.physics.add(col);
      out.push(col);
    }
    return out;
  }

  protected override addColliders(e: SimEntity) {
    const p = e.prefab as LoadedPrefab;
    if (!('colliders' in p)) return super.addColliders(e);
    const set: EntityColliders = { base: [], skin: [], conditional: [] };
    this.cols.set(e, set);
    set.base = this.addPrefabColliders(e, p, e.pose, p.isBuildingBlock);
  }

  protected override onSpawned(e: SimEntity) {
    const p = this.prefab(e);
    if (p.isBuildingBlock && p.construction) {
      // BuildingBlock.ServerInit -> UpdateSkin
      this.changeSkin(e);
    }
  }

  protected override onKilled(e: SimEntity) {
    // BuildingBlock.DestroyShared -> RefreshNeighbours(false)
    if (this.prefab(e).isBuildingBlock) this.refreshNeighbours(e, false);
    this.cols.delete(e);
  }

  gradeDef(e: SimEntity): GradeDef | null {
    const c = this.prefab(e).construction;
    if (!c) return null;
    return c.grades.find((g) => g.grade === e.grade && g.skin === e.skinID) ?? c.defaultGrade;
  }

  /** BuildingBlock.ChangeSkin (server side). */
  changeSkin(e: SimEntity) {
    if (e.destroyed) return;
    const set = this.cols.get(e);
    if (!set) return;
    const g = this.gradeDef(e);
    const skinId = g?.skinPrefab ?? this.prefab(e).construction?.defaultGrade?.skinPrefab ?? null;
    const skin = skinId != null ? this.data.prefabs.get(skinId) ?? null : null;
    const skinChanged = skinId !== e.skinPrefab;
    if (skinChanged) {
      for (const c of set.skin) this.physics.remove(c);
      set.skin = skin ? this.addPrefabColliders(e, skin, e.pose) : [];
      e.skinPrefab = skinId;
    }
    const conditionals = skin?.conditionals ?? [];
    const state = conditionals.map((cm: J) => runTests(this, e, cm.conditions ?? [], (iid) => (iid ? this.conditionalByIid.get(iid) ?? null : null)));
    const stateChanged = state.length !== e.modelState.length || state.some((v, i) => v !== e.modelState[i]);
    e.modelState = state;
    if (skinChanged || stateChanged) {
      for (const c of set.conditional) this.physics.remove(c);
      set.conditional = [];
      conditionals.forEach((cm: J, i: number) => {
        if (!state[i] || !cm.onServer || !cm.prefab?.prefab) return;
        const cp = this.data.prefabs.get(cm.prefab.prefab);
        if (!cp) return;
        const local = new Pose(Vector3.from(cm.worldPosition), Quaternion.from(cm.worldRotation));
        const pose = new Pose(e.pose.point(local.position), e.pose.rotation.mul(local.rotation));
        set.conditional.push(...this.addPrefabColliders(e, cp, pose));
      });
    }
    if (skinChanged) this.refreshNeighbours(e, true);
  }

  /** BuildingBlock.RefreshNeighbours */
  refreshNeighbours(e: SimEntity, linkToNeighbours: boolean) {
    for (const l of this.getEntityLinks(e, linkToNeighbours)) {
      for (const c of l.connections) if (this.prefab(c.owner).isBuildingBlock) this.skinQueue.add(c.owner);
    }
  }

  override cycle() {
    super.cycle();
    this.skinQueue.runQueue();
  }

  override settle(maxCycles = 10000) {
    for (let i = 0; i < maxCycles; i++) {
      if (this.stabilityCheckQueue.length === 0 && this.updateSurroundingsQueue.length === 0 && this.skinQueue.length === 0) return i;
      this.cycle();
    }
    throw new Error('did not settle');
  }

  // ---- placement -----------------------------------------------------------

  private ctx(): PlacementContext {
    return { lastPlacementError: '', world: this.physics, server: this };
  }

  /** Construction.UpdatePlacement */
  updatePlacement(prefab: LoadedPrefab, target: Target): PlacementResult {
    const con = prefab.construction!;
    const fail = (error: string, pl?: Placement, socket: SocketBase | null = null): PlacementResult => ({
      ok: false,
      error,
      position: pl?.position ?? target.position,
      rotation: pl?.rotation ?? Quaternion.identity,
      socket,
    });
    if (!target.valid) return fail(con.raw.placeOnWater ? 'Must be placed in water' : 'Invalid target');
    const ctx = this.ctx();
    let last: PlacementResult = fail('No valid socket');
    const males = prefab.sockets.filter((s) => s.male && !s.maleDummy && s.testTarget(target));
    for (const male of males) {
      if (target.entity && target.socket && this.isOccupied(target.entity, target.socket)) continue;
      ctx.lastPlacementError = '';
      const pl = male.doPlacement(target, ctx);
      if (!pl || !pl.isPopulated) {
        if (ctx.lastPlacementError) last = fail(ctx.lastPlacementError, undefined, male);
        continue;
      }
      if (!male.checkSocketMods(pl, ctx)) {
        last = fail(ctx.lastPlacementError || 'Socket check failed', pl, male);
        continue;
      }
      if (!this.testPlacingThroughRock(pl, target, con.bounds)) {
        last = fail('Cannot place through rock', pl, male);
        continue;
      }
      if (this.hasAlternativeLOSChecks(prefab)) {
        if (target.socket == null && !this.testPlacingThroughWall(pl, prefab, target)) {
          last = fail('Cannot place through walls', pl, male);
          continue;
        }
        if (!this.hasLineOfSight(pl, prefab, target)) {
          last = fail('Line of sight blocked', pl, male);
          continue;
        }
      } else if (!this.testPlacingThroughWall(pl, prefab, target)) {
        last = fail('Cannot place through walls', pl, male);
        continue;
      }
      if (target.entity && this.classesOf(target.entity).includes('Door') && target.socket == null) {
        last = fail('Cannot deploy on a door', pl, male);
        continue;
      }
      if (Vector3.distance(pl.position, target.playerEyes) > con.maxplaceDistance + 1) {
        last = fail('Too far away', pl, male);
        continue;
      }
      const blocked = checkVolumes(this, pl.position, pl.rotation, this.volumes(prefab));
      if (blocked) {
        const hit = blocked.lastHit?.entity;
        last = fail(hit ? `Blocked by ${this.displayName(hit)}` : 'Not enough space', pl, male);
        continue;
      }
      const prox = this.buildingProximity(prefab, pl.position, pl.rotation);
      if (prox) {
        last = fail(prox, pl, male);
        continue;
      }
      return { ok: true, error: '', position: pl.position, rotation: pl.rotation, socket: male };
    }
    return last;
  }

  displayName(e: SimEntity): string {
    const p = this.prefab(e);
    return p.construction?.name ?? p.path.split('/').pop()!.replace('.prefab', '');
  }

  private hasAlternativeLOSChecks(p: LoadedPrefab) {
    const c = p.construction!.raw;
    return !!c.alternativeLOSChecks && Array.isArray(c.alternativeLOSPositions) && c.alternativeLOSPositions.length > 0;
  }

  /** Construction.TestPlacingThroughRock (World layer = 65536). */
  private testPlacingThroughRock(pl: Placement, target: Target, bounds: import('./unity').Bounds): boolean {
    const obb = OBB.fromBounds(pl.position, pl.rotation, bounds);
    const center = this.player.position.add(new Vector3(0, 0.55, 0)); // GetCenter(ducked: true)
    const origin = target.ray.origin;
    if (this.physics.linecast(center, origin, 65536, QueryTriggerInteraction.Ignore)) return false;
    const hit = obb.trace(target.ray);
    const end = hit ? hit.point : obb.closestPoint(origin);
    return !this.physics.linecast(origin, end, 65536, QueryTriggerInteraction.Ignore);
  }

  /** Construction.TestPlacingThroughWall (Construction layer). */
  private testPlacingThroughWall(pl: Placement, prefab: LoadedPrefab, target: Target): boolean {
    const position = pl.position; // deployOffset transforms are not in this dump version
    const v = position.sub(target.ray.origin);
    const hit = this.physics.raycast(new Ray(target.ray.origin, v), v.magnitude, 2097152);
    if (!hit) return true;
    const he = hit.collider.entity;
    const enforce = !!prefab.construction!.raw.enforceLineOfSightCheckAgainstParentEntity;
    if (!enforce && he && he.isStability && target.entity === he) return true;
    return v.magnitude - hit.distance < 0.2;
  }

  /** Planner.HasLineOfSight (used by constructions with alternative LOS checks). Simplified: eye-to-point ray. */
  private hasLineOfSight(pl: Placement, prefab: LoadedPrefab, target: Target): boolean {
    const positions: number[][] = prefab.construction!.raw.alternativeLOSPositions ?? [];
    const eyes = this.player.eyes;
    for (const lp of positions) {
      const p = pl.position.add(pl.rotation.rotate(Vector3.from(lp)));
      const d = p.sub(eyes);
      const hit = this.physics.raycast(new Ray(eyes, d), Math.max(0, d.magnitude - 0.01), 2097152 | 65536, QueryTriggerInteraction.Ignore);
      if (!hit || (target.entity && hit.collider.entity === target.entity)) return true;
    }
    return positions.length === 0;
  }

  // ---- BuildingProximity ---------------------------------------------------

  /** BuildingProximity.Check — returns an error or null. */
  buildingProximity(prefab: LoadedPrefab, position: Vector3, rotation: Quaternion): string | null {
    const con = prefab.construction!;
    const obb = OBB.fromBounds(position, rotation, con.bounds);
    const radius = obb.extents.magnitude + 2;
    for (const bb of this.visEntities(obb.position, radius, 2097152)) {
      if (!this.prefab(bb).isBuildingBlock) continue;
      const other = this.prefab(bb);
      const p1 = proximity(prefab, position, rotation, other, bb.pose.position, bb.pose.rotation);
      const p2 = proximity(other, bb.pose.position, bb.pose.rotation, prefab, position, rotation);
      const hit = p1.hit || p2.hit;
      const line = p1.sqrDist <= p2.sqrDist ? p1.line : p2.line;
      // connection branch only matters with competing tool cupboards (not modelled yet)
      if (hit && line) {
        const v = line[1].sub(line[0]);
        if (!(Math.abs(v.y) > 1.49) && !(v.magnitude2D() > 1.49)) return `Too close to ${other.construction?.name ?? 'another block'}`;
      }
    }
    return null;
  }

  // ---- Planner.DoBuild -----------------------------------------------------

  /** Planner.DoBuild(target, construction) + DoPlacement. Returns the entity or an error. */
  build(prefab: LoadedPrefab, target: Target): { entity: SimEntity | null; error: string; result?: PlacementResult } {
    if (target.socket) {
      if (!target.socket.female) return { entity: null, error: 'Target socket is not female' };
      if (target.entity && this.isOccupied(target.entity, target.socket)) return { entity: null, error: 'Socket occupied' };
      if (target.onTerrain) return { entity: null, error: 'Target on terrain' };
    }
    const result = this.updatePlacement(prefab, target);
    if (!result.ok) return { entity: null, error: result.error, result };
    const e = this.spawnPlaced(prefab, new Pose(result.position, result.rotation), target);
    return { entity: e, error: '', result };
  }

  /** Spawn as Planner.DoPlacement does: default grade, health, then UpdateSurroundingEntities. */
  spawnPlaced(prefab: LoadedPrefab, pose: Pose, target?: Target, grade?: { grade: number; skin: number }): SimEntity {
    const deployable = prefab.construction?.deployable;
    const parent = deployable && target?.entity && shouldParent(this, target.entity, deployable) ? target.entity : null;
    const e = this.spawnWith(prefab, pose, parent, (ent) => {
      if (prefab.isBuildingBlock) {
        const g = grade ?? { grade: prefab.construction?.defaultGrade?.grade ?? 0, skin: prefab.construction?.defaultGrade?.skin ?? 0 };
        ent.grade = g.grade;
        ent.skinID = g.skin;
        ent.health = this.gradeDef(ent)?.baseHealth ?? 0;
      }
    });
    if (e.isStability) this.updateSurroundingsQueue.add(e.worldSpaceBounds().toBounds());
    return e;
  }

  /** Like World.spawn but lets the caller set state before ServerInit runs. */
  private spawnWith(prefab: LoadedPrefab, pose: Pose, parent: SimEntity | null, init: (e: SimEntity) => void): SimEntity {
    const pre = this.preInit;
    this.preInit = init;
    try {
      return this.spawn(prefab, pose, parent);
    } finally {
      this.preInit = pre;
    }
  }
  private preInit: ((e: SimEntity) => void) | null = null;

  protected override beforeServerInit(e: SimEntity) {
    this.preInit?.(e);
  }

  // ---- hammer actions ------------------------------------------------------

  /** BuildingBlock.CanChangeToGrade (no privilege model: always authorised). */
  canUpgrade(e: SimEntity, grade: number, skin: number): string | null {
    if (grade < e.grade) return 'Cannot downgrade';
    if (grade === e.grade && skin === e.skinID) return 'Already this grade';
    const con = this.prefab(e).construction!;
    if (!con.grades.some((g) => g.grade === grade && g.skin === skin)) return 'No such grade';
    if (con.raw.checkVolumeOnUpgrade) {
      const layer = this.prefab(e).layer;
      const blocked = checkVolumes(this, e.pose.position, e.pose.rotation, this.volumes(this.prefab(e)), ~(1 << layer));
      if (blocked) return 'Upgrade blocked';
    }
    return null;
  }

  /** DoUpgradeToGrade -> ChangeGrade -> UpdateSkin */
  upgrade(e: SimEntity, grade: number, skin = 0): string | null {
    const err = this.canUpgrade(e, grade, skin);
    if (err) return err;
    e.grade = grade;
    e.skinID = skin;
    e.health = this.gradeDef(e)?.baseHealth ?? e.health;
    this.changeSkin(e);
    return null;
  }

  /** BuildingBlock.CanRotate / DoRotation */
  rotate(e: SimEntity): string | null {
    const con = this.prefab(e).construction;
    if (!con || !con.raw.canRotateAfterPlacement) return 'Cannot rotate';
    if (con.raw.checkVolumeOnRotate) {
      const layer = this.prefab(e).layer;
      if (checkVolumes(this, e.pose.position, e.pose.rotation, this.volumes(this.prefab(e)), ~(1 << layer))) return 'Rotation blocked';
    }
    const newPose = new Pose(e.pose.position, e.pose.rotation.mul(Quaternion.euler(con.rotationAmount)));
    this.movePose(e, newPose);
    this.refreshEntityLinks(e);
    this.updateSurroundingsQueue.add(e.worldSpaceBounds().toBounds());
    this.changeSkin(e);
    this.refreshNeighbours(e, false);
    return null;
  }

  /** Rebuilds all colliders of an entity at a new pose. */
  private movePose(e: SimEntity, pose: Pose) {
    const set = this.cols.get(e);
    if (set) for (const c of [...set.base, ...set.skin, ...set.conditional]) this.physics.remove(c);
    e.pose = pose;
    e.skinPrefab = null;
    e.modelState = [];
    if (set) {
      set.skin = [];
      set.conditional = [];
      set.base = this.addPrefabColliders(e, this.prefab(e), pose, this.prefab(e).isBuildingBlock);
    }
  }

  demolish(e: SimEntity) {
    this.kill(e, 'manual');
  }
}

// ---------------------------------------------------------------------------

type Line = [Vector3, Vector3];

/** BuildingProximity.GetProximity */
function proximity(c1: LoadedPrefab, pos1: Vector3, rot1: Quaternion, c2: LoadedPrefab, pos2: Vector3, rot2: Quaternion) {
  const res = { hit: false, connection: false, line: null as Line | null, sqrDist: Number.MAX_VALUE };
  for (const s1 of c1.sockets) {
    if (!(s1 instanceof ConstructionSocket)) continue;
    for (const s2 of c2.sockets) if (s1.canConnect(pos1, rot1, s2, pos2, rot2)) return { ...res, connection: true };
  }
  for (const s1 of c1.sockets) {
    if (!(s1 instanceof NeighbourSocket) && !(s1 instanceof StabilitySocket)) continue;
    for (const s2 of c2.sockets) if (s1.canConnect(pos1, rot1, s2, pos2, rot2)) return { ...res, connection: true };
  }
  const prox1 = c1.construction?.proximities ?? [];
  const prox2 = c2.construction?.proximities ?? [];
  if (prox1.length !== 0) {
    for (const s1 of c1.sockets) {
      if (!(s1 instanceof ConstructionSocket) || s1.socketType !== ConstructionSocketType.Wall) continue;
      const a = s1.getSelectPivot(pos1, rot1);
      for (const p of prox2) {
        const b = pos2.add(rot2.rotate(p.worldPosition));
        const d = b.sub(a).sqrMagnitude;
        if (d < res.sqrDist) {
          res.hit = true;
          res.line = [a, b];
          res.sqrDist = d;
        }
      }
    }
  }
  return res;
}

/** Planner.ShouldParent */
function shouldParent(srv: BuildServer, target: SimEntity, deployable: J): boolean {
  // SupportsChildDeployables(): building blocks don't; deployables with sockets for children do.
  const classes = srv.classesOf(target);
  if (classes.includes('BuildingBlock')) return false;
  return !!deployable.setSocketParent;
}
