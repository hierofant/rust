// Entity container + server-side lifecycle: entity links (BaseEntity.LinkToNeighbours),
// stability (StabilityEntity.cs) and the work queues ServerBuildingManager.Cycle runs.
import { INT_MAX, SimEntity, Support, type EntityLink, type PrefabDef } from './entity';
import { ConstructionSocket, StabilitySocket, type SocketBase } from './sockets';
import { Bounds, Pose, Vector3, clamp01 } from './unity';
import { ObjectWorkQueue } from './workqueue';

/** ConVar.Stability defaults. */
export interface StabilityConfig {
  enabled: boolean; // ConVar.Server.stability
  strikes: number;
  collapse: number;
  accuracy: number;
  supportHighestStability: boolean;
  /** Max jobs per Cycle (the game uses a 9 ms time budget; Infinity = drain fully). */
  stabilityQueueBudget: number;
  surroundingsQueueBudget: number;
}

export const defaultStability: StabilityConfig = {
  enabled: true,
  strikes: 10,
  collapse: 0.05,
  accuracy: 0.001,
  supportHighestStability: false,
  stabilityQueueBudget: Infinity,
  surroundingsQueueBudget: Infinity,
};

export type WorldEvent =
  | { type: 'spawn'; entity: SimEntity }
  | { type: 'kill'; entity: SimEntity; reason: 'stability' | 'manual' }
  | { type: 'stability'; entity: SimEntity };

export class World {
  entities: SimEntity[] = [];
  config: StabilityConfig;
  listeners: ((e: WorldEvent) => void)[] = [];

  readonly stabilityCheckQueue: ObjectWorkQueue<SimEntity>;
  readonly updateSurroundingsQueue: ObjectWorkQueue<Bounds>;

  constructor(config: Partial<StabilityConfig> = {}) {
    this.config = { ...defaultStability, ...config };
    this.stabilityCheckQueue = new ObjectWorkQueue<SimEntity>(
      (e) => {
        if (this.shouldAddStability(e)) this.stabilityCheck(e);
      },
      (e) => this.shouldAddStability(e),
    );
    this.updateSurroundingsQueue = new ObjectWorkQueue<Bounds>(
      (b) => this.notifyNeighbours(b),
      () => true,
      () => true,
    );
  }

  private emit(e: WorldEvent) {
    for (const l of this.listeners) l(e);
  }

  // ---- queries -------------------------------------------------------------

  /**
   * Stand-in for Vis.Entities(position, radius): entities whose world bounds touch the sphere.
   * The game queries colliders; bounds are a superset for building pieces.
   */
  entitiesInSphere(center: Vector3, radius: number): SimEntity[] {
    const r2 = radius * radius;
    return this.entities.filter((e) => !e.destroyed && e.worldSpaceBounds().closestPoint(center).sub(center).sqrMagnitude <= r2);
  }

  // ---- lifecycle -----------------------------------------------------------

  spawn(prefab: PrefabDef, pose: Pose, parent: SimEntity | null = null): SimEntity {
    const e = new SimEntity(prefab, pose);
    e.parent = parent;
    this.entities.push(e);
    // StabilityEntity.ServerInit
    if (e.isStability) this.updateStability(e);
    this.emit({ type: 'spawn', entity: e });
    return e;
  }

  kill(e: SimEntity, reason: 'stability' | 'manual' = 'manual') {
    if (e.destroyed) return;
    e.destroyed = true;
    // DoServerDestroy -> UpdateSurroundingEntities
    if (e.isStability) this.updateSurroundingsQueue.add(e.worldSpaceBounds().toBounds());
    // DestroyShared -> FreeEntityLinks
    for (const l of e.links) l.clear();
    e.links = [];
    e.linkedToNeighbours = false;
    this.entities = this.entities.filter((x) => x !== e);
    this.emit({ type: 'kill', entity: e, reason });
  }

  /** ServerBuildingManager.Cycle (the stability part). */
  cycle() {
    this.stabilityCheckQueue.runQueue(this.config.stabilityQueueBudget);
    this.updateSurroundingsQueue.runQueue(this.config.surroundingsQueueBudget);
  }

  /** Run cycles until both queues are empty. */
  settle(maxCycles = 10000) {
    for (let i = 0; i < maxCycles; i++) {
      if (this.stabilityCheckQueue.length === 0 && this.updateSurroundingsQueue.length === 0) return i;
      this.cycle();
    }
    throw new Error('stability did not settle');
  }

  // ---- entity links --------------------------------------------------------

  getEntityLinks(e: SimEntity, linkToNeighbours = true): EntityLink[] {
    if (!e.linkedToNeighbours && linkToNeighbours) this.linkToNeighbours(e);
    return e.links;
  }

  isOccupied(e: SimEntity, socket: SocketBase): boolean {
    const link = this.getEntityLinks(e).find((l) => l.socket === socket);
    return link?.isOccupied() ?? false;
  }

  private linkToEntity(a: SimEntity, b: SimEntity) {
    if (a === b || a.links.length === 0 || b.links.length === 0) return;
    for (const la of a.links) {
      for (const lb of b.links) {
        if (la.canConnect(lb)) {
          if (!la.contains(lb)) la.add(lb);
          if (!lb.contains(la)) lb.add(la);
        }
      }
    }
  }

  private linkToNeighbours(e: SimEntity) {
    if (e.links.length === 0) return;
    e.linkedToNeighbours = true;
    const obb = e.worldSpaceBounds();
    for (const other of this.entitiesInSphere(obb.position, obb.extents.magnitude + 1)) this.linkToEntity(e, other);
  }

  refreshEntityLinks(e: SimEntity) {
    for (const l of e.links) l.clear();
    this.linkToNeighbours(e);
  }

  // ---- stability (StabilityEntity.cs) -------------------------------------

  private shouldAddStability(e: SimEntity) {
    return this.config.enabled && !e.destroyed;
  }

  updateStability(e: SimEntity, depth = 0) {
    e.stabilityUpdateDepth = depth;
    this.stabilityCheckQueue.add(e);
  }

  private initializeSupports(e: SimEntity) {
    e.supports = [];
    // HasParent() && !parent.AllowInitChildSupports(): parented pieces get no supports.
    if (e.grounded || e.parent != null) return;
    for (const link of this.getEntityLinks(e)) {
      if (!link.isMale()) continue;
      if (link.socket instanceof StabilitySocket) e.supports.push(new Support(e, link, link.socket.support));
      if (link.socket instanceof ConstructionSocket) e.supports.push(new Support(e, link, link.socket.support));
    }
  }

  /** Support.SupportEntity */
  private supportEntity(support: Support, ignore: SimEntity | null): SimEntity | null {
    let best: SimEntity | null = null;
    for (const c of support.link.connections) {
      const other = c.owner;
      const socket = c.socket;
      if (!other.isStability || other === support.parent || other === ignore || other.destroyed) continue;
      if (socket instanceof ConstructionSocket && socket.femaleNoStability) continue;
      if (best == null) best = other;
      else if (this.config.supportHighestStability) {
        if (other.cachedStability > best.cachedStability) best = other;
      } else if (other.cachedDistanceFromGround < best.cachedDistanceFromGround) best = other;
    }
    return best;
  }

  private forcesFull(e: SimEntity) {
    // ParentForcesFullStability(): depends on the parent entity type; parented pieces are rare in bases.
    return e.grounded;
  }

  private distanceFromGround(e: SimEntity, cached: boolean, ignore: SimEntity | null = null): number {
    if (this.forcesFull(e)) return 1;
    if (e.supports == null) return 1;
    ignore ??= e;
    let num = INT_MAX;
    for (const s of e.supports) {
      const se = this.supportEntity(s, ignore);
      if (se == null) continue;
      const d = cached ? se.cachedDistanceFromGround : this.distanceFromGround(se, true, ignore);
      if (d !== INT_MAX) num = Math.min(num, d + 1);
    }
    return num;
  }

  private supportValue(e: SimEntity, cached: boolean, ignore: SimEntity | null = null): { value: number; supportEntity: SimEntity | null } {
    let supportEntity: SimEntity | null = null;
    if (this.forcesFull(e)) return { value: 1, supportEntity };
    if (e.supports == null) return { value: 1, supportEntity };
    ignore ??= e;
    let num = 0;
    for (const s of e.supports) {
      const se = this.supportEntity(s, ignore);
      if (!cached) supportEntity = se;
      if (se == null) continue;
      const v = cached ? se.cachedStability : this.supportValue(se, true, ignore).value;
      if (v !== 0) num += v * s.factor;
    }
    return { value: clamp01(num), supportEntity };
  }

  /** StabilityEntity.StabilityCheck */
  stabilityCheck(e: SimEntity) {
    if (e.destroyed) return;
    if (e.supports == null) this.initializeSupports(e);
    let changed = false;
    const dist = this.distanceFromGround(e, false);
    if (dist !== e.cachedDistanceFromGround) {
      e.cachedDistanceFromGround = dist;
      if (!this.config.supportHighestStability) changed = true;
    }
    const { value } = this.supportValue(e, false);
    if (Math.abs(e.cachedStability - value) > this.config.accuracy) {
      e.cachedStability = value;
      changed = true;
    }
    if (changed) {
      e.dirty = true;
      this.updateConnectedEntities(e);
      this.updateStability(e, e.stabilityUpdateDepth + 1);
      this.emit({ type: 'stability', entity: e });
    } else if (e.dirty) {
      e.dirty = false;
    }
    if (value < this.config.collapse) {
      if (e.stabilityStrikes < this.config.strikes) {
        this.updateStability(e, e.stabilityUpdateDepth + 1);
        e.stabilityStrikes++;
        return;
      }
      this.kill(e, 'stability');
    } else {
      e.stabilityStrikes = 0;
    }
  }

  private updateConnectedEntities(e: SimEntity) {
    for (const link of this.getEntityLinks(e)) {
      if (!link.isFemale()) continue;
      for (const c of link.connections) {
        const other = c.owner;
        if (other.isStability && !other.destroyed) this.updateStability(other, e.stabilityUpdateDepth + 1);
      }
    }
  }

  /** UpdateSurroundingsQueue.NotifyNeighbours */
  private notifyNeighbours(bounds: Bounds) {
    if (!this.config.enabled) return;
    for (const e of this.entitiesInSphere(bounds.center, bounds.extents.magnitude + 1)) {
      if (!e.destroyed && e.isStability) this.stabilityCheck(e);
    }
  }
}
