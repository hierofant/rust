// Server-side entity state relevant to building: pose, entity links (BaseEntity links,
// EntityLink.cs, EntityLinkEx.cs) and StabilityEntity fields.
import { OBB } from './obb';
import type { SocketBase } from './sockets';
import { Bounds, Pose } from './unity';

export const INT_MAX = 2147483647;

export interface PrefabDef {
  prefabID: number;
  path: string;
  /** PrefabAttribute.server.FindAll<Socket_Base>(prefabID), in dump order. */
  sockets: SocketBase[];
  /** BaseEntity.bounds */
  bounds: Bounds;
  /** Entity derives from StabilityEntity. */
  isStabilityEntity: boolean;
  /** StabilityEntity.grounded */
  grounded: boolean;
}

export class EntityLink {
  connections: EntityLink[] = [];
  capacity: number;

  constructor(public readonly owner: SimEntity, public readonly socket: SocketBase) {
    this.capacity = socket.monogamous ? 1 : INT_MAX;
  }

  get name() { return this.socket.socketName; }
  contains(l: EntityLink) { return this.connections.includes(l); }
  add(l: EntityLink) { this.connections.push(l); }
  remove(l: EntityLink) {
    const i = this.connections.indexOf(l);
    if (i >= 0) this.connections.splice(i, 1);
  }
  clear() {
    for (const c of this.connections) c.remove(this);
    this.connections = [];
  }
  isEmpty() { return this.connections.length === 0; }
  isOccupied() { return this.connections.length >= this.capacity; }
  isMale() { return this.socket.male; }
  isFemale() { return this.socket.female; }

  canConnect(link: EntityLink | null): boolean {
    if (this.isOccupied()) return false;
    if (link == null) return false;
    if (link.isOccupied()) return false;
    const a = this.owner.pose, b = link.owner.pose;
    return this.socket.canConnect(a.position, a.rotation, link.socket, b.position, b.rotation);
  }
}

export class Support {
  constructor(public readonly parent: SimEntity, public readonly link: EntityLink, public readonly factor: number) {}
}

let nextId = 1;

export class SimEntity {
  readonly id = nextId++;
  parent: SimEntity | null = null;
  destroyed = false;

  links: EntityLink[] = [];
  linkedToNeighbours = false;

  // StabilityEntity
  grounded: boolean;
  cachedStability = 0;
  cachedDistanceFromGround = INT_MAX;
  stabilityUpdateDepth = 0;
  supports: Support[] | null = null;
  stabilityStrikes = 0;
  dirty = false;

  constructor(public readonly prefab: PrefabDef, public pose: Pose) {
    this.grounded = prefab.grounded;
    // InitEntityLinks
    for (const s of prefab.sockets) this.links.push(new EntityLink(this, s));
  }

  get isStability() { return this.prefab.isStabilityEntity; }

  /** BaseEntity.WorldSpaceBounds() */
  worldSpaceBounds(): OBB {
    return OBB.fromBounds(this.pose.position, this.pose.rotation, this.prefab.bounds);
  }

  findLink(socket: SocketBase): EntityLink | null {
    return this.links.find((l) => l.socket === socket) ?? null;
  }

  isOccupied(socket: SocketBase): boolean {
    return this.findLink(socket)?.isOccupied() ?? false;
  }
}
