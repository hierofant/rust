// Ports of Socket_Base and subclasses (Assembly-CSharp/Socket_*.cs, ConstructionSocket.cs,
// NeighbourSocket.cs, StabilitySocket.cs). Field names match the C# originals so the
// runtime dump can be mapped 1:1.
import { OBB } from './obb';
import { Bounds, Pose, Quaternion, Vector3 } from './unity';
import type { Placement, Target } from './construction';
import type { SocketMod } from './socketMods';
import type { PhysicsQueries } from './physics';

export interface OccupiedSocketCheck {
  socketName: string;
  femaleDummy: boolean;
}

/** Fields every PrefabAttribute carries, relative to the prefab root. */
export interface AttributeTransform {
  worldPosition: Vector3;
  worldRotation: Quaternion;
  localPosition: Vector3;
  localRotation: Quaternion;
}

export abstract class SocketBase implements AttributeTransform {
  /** Stands in for C# GetType(); IsCompatible requires both sockets to be the same concrete type. */
  abstract readonly kind: string;

  male = true;
  maleDummy = false;
  female = false;
  femaleDummy = false;
  femaleNoStability = false;
  monogamous = false;
  selectSize = new Vector3(2, 0.1, 2);
  selectCenter = new Vector3(0, 0, 1);
  socketName = '';
  socketMods: SocketMod[] = [];
  checkOccupiedSockets: OccupiedSocketCheck[] = [];

  worldPosition = Vector3.zero;
  worldRotation = Quaternion.identity;
  localPosition = Vector3.zero;
  localRotation = Quaternion.identity;
  /** Set in AttributeSetup from transform.position/rotation — same space as worldPosition/worldRotation. */
  get position() { return this.worldPosition; }
  get rotation() { return this.worldRotation; }

  getSelectPivot(position: Vector3, rotation: Quaternion): Vector3 {
    return position.add(rotation.rotate(this.worldPosition));
  }

  getSelectBounds(position: Vector3, rotation: Quaternion): OBB {
    return OBB.fromBounds(
      position.add(rotation.rotate(this.worldPosition)),
      rotation.mul(this.worldRotation),
      new Bounds(this.selectCenter, this.selectSize),
    );
  }

  testTarget(target: Target): boolean {
    return target.socket != null;
  }

  isCompatible(socket: SocketBase | null): boolean {
    if (socket == null) return false;
    if (!socket.male && !this.male) return false;
    if (!socket.female && !this.female) return false;
    return socket.kind === this.kind;
  }

  canConnect(_position: Vector3, _rotation: Quaternion, socket: SocketBase, _socketPosition: Vector3, _socketRotation: Quaternion): boolean {
    return this.isCompatible(socket);
  }

  doPlacement(target: Target, _ctx: PlacementContext): Placement | null {
    const q = Quaternion.lookRotation(target.normal, Vector3.up).mul(Quaternion.euler(target.rotation));
    const pos = target.position.sub(q.rotate(this.position));
    return { ...newPlacement(target), rotation: q, position: pos };
  }

  checkSocketMods(placement: Placement, ctx: PlacementContext): boolean {
    for (const mod of this.socketMods) mod.modifyPlacement(placement, ctx);
    let sawAreaCheck = false;
    for (const mod of this.socketMods) {
      if (mod.socketGrouping != null) continue;
      if (placement.shouldParent && !sawAreaCheck && mod.kind === 'SocketMod_AreaCheck') sawAreaCheck = true;
      if (!mod.doCheck(placement, ctx)) {
        ctx.lastPlacementError = mod.errorMessage();
        return false;
      }
    }
    if (sawAreaCheck && placement.shouldParent && !placement.parentPassed) {
      ctx.lastPlacementError = 'NotStableEnough';
      return false;
    }
    return true;
  }
}

/** Mutable per-attempt state that the C# code keeps in statics (Construction.lastPlacementError etc). */
export interface PlacementContext {
  lastPlacementError: string;
  world: PhysicsQueries;
}

export function newPlacement(target: Target): Placement {
  return {
    position: Vector3.zero,
    rotation: Quaternion.identity,
    isPopulated: true,
    shouldParent: target.shouldParent,
    parentPassed: false,
    isHoldingShift: target.isHoldingShift,
    transform: target.entity ? target.entity.pose : null,
    ignoredEntity: null,
  };
}

export enum ConstructionSocketType {
  None, Foundation, Floor, Misc, Doorway, Wall, Block, Ramp, StairsTriangle, Stairs,
  FloorFrameTriangle, Window, Shutters, WallFrame, FloorFrame, WindowDressing, DoorDressing,
  Elevator, DoubleDoorDressing,
}

export enum ConstructionBlockType { Building, Boat }

const outsideLookupDirs = [new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1)];

export class ConstructionSocket extends SocketBase {
  kind = 'ConstructionSocket';
  socketType = ConstructionSocketType.None;
  blockType = ConstructionBlockType.Building;
  rotationDegrees = 0;
  rotationOffset = 0;
  restrictPlacementRotation = false;
  restrictPlacementAngle = false;
  faceAngle = 0;
  angleAllowed = 150;
  wantsInside = false;
  support = 1;

  override testTarget(target: Target): boolean {
    if (!super.testTarget(target)) return false;
    return this.isCompatible(target.socket);
  }

  override isCompatible(socket: SocketBase | null): boolean {
    if (!super.isCompatible(socket)) return false;
    if (!(socket instanceof ConstructionSocket)) return false;
    if (socket.socketType === ConstructionSocketType.None || this.socketType === ConstructionSocketType.None) return false;
    if (socket.socketType !== this.socketType) return false;
    if (socket.blockType !== this.blockType) return false;
    return true;
  }

  override canConnect(position: Vector3, rotation: Quaternion, socket: SocketBase, socketPosition: Vector3, socketRotation: Quaternion): boolean {
    if (!super.canConnect(position, rotation, socket, socketPosition, socketRotation)) return false;
    const m1 = new Pose(position, rotation);
    const m2 = new Pose(socketPosition, socketRotation);
    const a = m1.point(this.worldPosition);
    const b = m2.point(socket.worldPosition);
    if (Vector3.distance(a, b) > 0.02) return false;
    const v1 = m1.vector(this.worldRotation.rotate(Vector3.forward));
    const v2 = m2.vector(socket.worldRotation.rotate(Vector3.forward));
    let num = Vector3.angle(v1, v2);
    if (this.male && this.female) num = Math.min(num, Vector3.angle(v1.neg(), v2));
    if (socket.male && socket.female) num = Math.min(num, Vector3.angle(v1, v2.neg()));
    return num <= 2;
  }

  testRestrictedAngles(_suggestedPos: Vector3, suggestedAng: Quaternion, target: Target): boolean {
    if (this.restrictPlacementAngle) {
      const q = Quaternion.euler(0, this.faceAngle, 0).mul(suggestedAng);
      const num = Vector3.dotDegrees(target.ray.direction.xz3d(), q.rotate(Vector3.forward));
      if (num > this.angleAllowed * 0.5) return false;
      if (num < this.angleAllowed * -0.5) return false;
    }
    return true;
  }

  override doPlacement(target: Target, ctx: PlacementContext): Placement | null {
    if (!target.entity) return null;
    if (!this.canConnectToEntity(target)) return null;
    const cs = target.socket instanceof ConstructionSocket ? target.socket : null;
    const vector = targetWorldPosition(target);
    const quaternion = targetWorldRotation(target, true);
    if (cs != null && !this.isCompatible(cs)) return null;
    if (this.wantsInside) {
      const e = target.entity.pose;
      const pos = e.position.add(this.localPosition).add(e.rotation.rotate(Vector3.right).mul(0.2));
      if (this.isOutside(pos, e, ctx)) {
        ctx.lastPlacementError = 'WantsInside';
        return null;
      }
    }
    if (this.rotationDegrees > 0 && (cs == null || !cs.restrictPlacementRotation)) {
      const placement = newPlacement(target);
      let best = Number.MAX_VALUE;
      let bestI = 0;
      for (let i = 0; i < 360; i += this.rotationDegrees) {
        const q2 = Quaternion.euler(0, this.rotationOffset + i, 0);
        const to = q2.mul(quaternion).rotate(Vector3.up);
        const a = Vector3.angle(target.ray.direction, to);
        if (a < best) { best = a; bestI = i; }
      }
      for (let j = 0; j < 360; j += this.rotationDegrees) {
        const q3 = quaternion.mul(this.rotation.inverse());
        const q4 = Quaternion.euler(target.rotation);
        const q5 = Quaternion.euler(0, this.rotationOffset + j + bestI, 0);
        const q6 = q4.mul(q5).mul(q3);
        placement.position = vector.sub(q6.rotate(this.position));
        placement.rotation = q6;
        if (this.checkSocketMods(placement, ctx)) return placement;
      }
    }
    const result = newPlacement(target);
    let q7 = quaternion.mul(this.rotation.inverse());
    if (shouldInheritFemaleSocketRotation(this.socketType)) q7 = q7.mul(Quaternion.euler(target.rotation));
    result.position = vector.sub(q7.rotate(this.position));
    result.rotation = q7;
    if (!this.testRestrictedAngles(vector, quaternion, target)) return null;
    return result;
  }

  protected canConnectToEntity(_target: Target): boolean {
    return true;
  }

  /** Raycasts against the Construction layer (2097152) in 4 directions. */
  isOutside(pos: Vector3, tr: Pose, ctx: PlacementContext): boolean {
    const raycast = (o: Vector3, d: Vector3, dist: number, mask: number) => ctx.world.raycastAny(o, d, dist, mask);
    const num = 5;
    for (const d of outsideLookupDirs) {
      const v = tr.vector(d);
      const origin = pos.add(v.mul(num));
      if (!raycast(origin, v.neg(), num - 0.5, 2097152)) return true;
    }
    return false;
  }
}

function shouldInheritFemaleSocketRotation(type: ConstructionSocketType): boolean {
  return (
    type === ConstructionSocketType.WallFrame ||
    type === ConstructionSocketType.Doorway ||
    type === ConstructionSocketType.FloorFrameTriangle ||
    type === ConstructionSocketType.FloorFrame ||
    type === ConstructionSocketType.Window ||
    type === ConstructionSocketType.Shutters
  );
}

/** Construction.Target.GetWorldPosition */
export function targetWorldPosition(target: Target): Vector3 {
  return target.entity!.pose.point(target.socket!.position);
}

/** Construction.Target.GetWorldRotation */
export function targetWorldRotation(target: Target, female: boolean): Quaternion {
  const s = target.socket!;
  let q = s.rotation;
  if (s.male && s.female && female) q = s.rotation.mul(Quaternion.euler(180, 0, 180));
  return target.entity!.pose.rotation.mul(q);
}

abstract class SelectBoundsSocket extends SocketBase {
  override testTarget(): boolean {
    return false;
  }
  override canConnect(position: Vector3, rotation: Quaternion, socket: SocketBase, socketPosition: Vector3, socketRotation: Quaternion): boolean {
    if (!super.canConnect(position, rotation, socket, socketPosition, socketRotation)) return false;
    return this.getSelectBounds(position, rotation).intersects(socket.getSelectBounds(socketPosition, socketRotation));
  }
}

export class NeighbourSocket extends SelectBoundsSocket {
  kind = 'NeighbourSocket';
}

export class StabilitySocket extends SelectBoundsSocket {
  kind = 'StabilitySocket';
  support = 1;
}

export class SocketSpecificFemale extends SocketBase {
  kind = 'Socket_Specific_Female';
  rotationDegrees = 0;
  rotationOffset = 0;
  allowedMaleSockets: string[] = [];
  parentToBone = false;
  boneName = '';

  canAccept(socket: SocketSpecific): boolean {
    return this.allowedMaleSockets.includes(socket.targetSocketName);
  }
}

export class SocketSpecific extends SocketBase {
  kind = 'Socket_Specific';
  useFemaleRotation = true;
  targetSocketName = '';
  blockPlacementOnChildEntities = false;
  canRotate = false;

  override testTarget(target: Target): boolean {
    if (!super.testTarget(target)) return false;
    const female = target.socket instanceof SocketSpecificFemale ? target.socket : null;
    if (female == null) return false;
    if (this.blockPlacementOnChildEntities && target.entity != null && target.entity.parent != null) return false;
    // SocketMod_BoatBuildingBlock.Contained(...) check omitted: boats are out of scope.
    return female.canAccept(this);
  }

  override doPlacement(target: Target, _ctx: PlacementContext): Placement | null {
    const socket = target.socket!;
    let q = socket.rotation;
    if (socket.male && socket.female) q = socket.rotation.mul(Quaternion.euler(180, 0, 180));
    // parentToBone: bones are not simulated; entity root transform is used.
    const tr = target.entity!.pose;
    const vector = tr.point(socket.localPosition);
    let q2: Quaternion;
    if (this.useFemaleRotation) {
      q2 = tr.rotation.mul(q);
    } else {
      const v2 = new Vector3(vector.x, 0, vector.z);
      const eyes = target.playerEyes;
      const v3 = new Vector3(eyes.x, 0, eyes.z);
      q2 = Quaternion.lookRotation(v2.sub(v3).normalized).mul(q);
    }
    const result = newPlacement(target);
    let q3 = q2.mul(this.rotation.inverse());
    if (this.canRotate) q3 = q3.mul(Quaternion.euler(target.rotation));
    result.position = vector.sub(q3.rotate(this.position));
    result.rotation = q3;
    return result;
  }
}

export class SocketFree extends SocketBase {
  kind = 'Socket_Free';
  idealPlacementNormal = Vector3.up;
  useTargetNormal = true;
  snapToTargetProvidedRotations = false;
  blendAimAngle = true;

  override testTarget(target: Target): boolean {
    return target.onTerrain;
  }

  override doPlacement(target: Target, _ctx: PlacementContext): Placement | null {
    let q: Quaternion;
    // snapToTargetProvidedRotations (IPlacementDirectionProvider) not ported yet.
    if (this.useTargetNormal) {
      const normal = target.normal;
      let upwards = this.idealPlacementNormal;
      if (this.blendAimAngle || Math.abs(target.normal.y) > 0.98) {
        const n = target.position.sub(target.ray.origin).normalized;
        upwards = Vector3.lerp(n, this.idealPlacementNormal, Math.abs(Vector3.dot(n, normal)));
      }
      q = Quaternion.lookRotation(normal, upwards).mul(this.rotation.inverse()).mul(Quaternion.euler(target.rotation));
    } else {
      const n = target.position.sub(target.ray.origin).normalized.withY(0);
      q = Quaternion.lookRotation(n, this.idealPlacementNormal).mul(Quaternion.euler(target.rotation));
    }
    const result = newPlacement(target);
    result.rotation = q;
    result.position = target.position.sub(q.rotate(this.position));
    return result;
  }
}
