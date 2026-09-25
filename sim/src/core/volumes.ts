// DeployVolume ports (Assembly-CSharp/DeployVolume*.cs): the "can this go here" volumes of a
// prefab, tested against colliders of other objects on the volume's layers.
import type { SimEntity } from './entity';
import { OBB } from './obb';
import { QueryTriggerInteraction, type Collider, type PhysicsWorld } from './physics';
import { checkEntityList } from './socketMods';
import { Bounds, Quaternion, Vector3 } from './unity';

type J = any;

export const ColliderFlags = {
  OnlyBlockBuildingBlock: 0x20,
  Monument: 0x40,
  OnlyBlockDeployables: 0x200,
  OnlyEvaluatePreventBuildingInMonuments: 0x400,
} as const;

export interface VolumeServer {
  physics: PhysicsWorld;
  visEntities(center: Vector3, radius: number, layerMask?: number, qti?: QueryTriggerInteraction): SimEntity[];
  volumesOf(e: SimEntity): DeployVolume[];
}

const vec = (a: number[] | undefined, d = Vector3.zero) => (a ? new Vector3(a[0], a[1], a[2]) : d);
const quat = (a: number[] | undefined) => (a ? new Quaternion(a[0], a[1], a[2], a[3]) : Quaternion.identity);
const bounds = (b: J) => new Bounds(vec(b?.center), vec(b?.size, Vector3.one));

export class DeployVolume {
  readonly kind: string;
  readonly layers: number;
  readonly ignore: number;
  readonly entityMode: number;
  readonly entityList: J[];
  readonly entityGroups: J[];
  readonly worldPosition: Vector3;
  readonly worldRotation: Quaternion;
  /** DeployVolume.IsBuildingBlock: the owning prefab is a BuildingBlock. */
  readonly isBuildingBlock: boolean;
  lastHit: Collider | null = null;

  constructor(readonly j: J, isBuildingBlock: boolean) {
    this.kind = j.$type;
    this.layers = j.layers ?? 537001984;
    this.ignore = j.ignore ?? 0;
    this.entityMode = j.entityMode ?? 0;
    this.entityList = j.entityList ?? [];
    this.entityGroups = j.entityGroups ?? [];
    this.worldPosition = vec(j.worldPosition);
    this.worldRotation = quat(j.worldRotation);
    this.isBuildingBlock = isBuildingBlock;
  }

  /** DeployVolume.Check(position, rotation, mask) — true means blocked. */
  check(srv: VolumeServer, position: Vector3, rotation: Quaternion, mask = -1): boolean {
    const j = this.j;
    const layers = this.layers & mask;
    switch (this.kind) {
      case 'DeployVolumeOBB': {
        const b = bounds(j.bounds);
        const pos = position.add(rotation.rotate(this.worldRotation.rotate(b.center).add(this.worldPosition)));
        return this.checkColliders(srv.physics.overlapOBB(OBB.fromSize(pos, b.size, rotation.mul(this.worldRotation)), layers, QueryTriggerInteraction.Collide));
      }
      case 'DeployVolumeSphere': {
        const pos = position.add(rotation.rotate(this.worldRotation.rotate(vec(j.center)).add(this.worldPosition)));
        return this.checkColliders(srv.physics.overlapSphere(pos, j.radius ?? 0.5, layers, QueryTriggerInteraction.Collide));
      }
      case 'DeployVolumeCapsule': {
        const pos = position.add(rotation.rotate(this.worldRotation.rotate(vec(j.center)).add(this.worldPosition)));
        const up = rotation.mul(this.worldRotation).rotate(Vector3.up).mul((j.height ?? 1) * 0.5);
        return this.checkColliders(srv.physics.overlapCapsule(pos.add(up), pos.sub(up), j.radius ?? 0.5, layers, QueryTriggerInteraction.Collide));
      }
      case 'DeployVolumeEntityBounds': {
        const b = bounds(j.bounds);
        const pos = position.add(rotation.rotate(b.center));
        return this.checkColliders(srv.physics.overlapOBB(OBB.fromSize(pos, b.size, rotation), layers, QueryTriggerInteraction.Collide));
      }
      case 'DeployVolumeEntityBoundsReverse': {
        const b = bounds(j.bounds);
        const pos = position.add(rotation.rotate(b.center));
        const test = OBB.fromSize(pos, b.size, rotation);
        for (const item of srv.visEntities(pos, test.extents.magnitude, layers)) {
          const vols = srv.volumesOf(item).filter((v) => shouldApplyVolumeForEntity(v, item));
          if (vols.some((v) => v.checkOBB(item.pose.position, item.pose.rotation, test, 1 << (j.layer ?? 0)))) return true;
        }
        return false;
      }
      case 'DeployVolumeRequireBoatBuildingVolume':
        // Only valid inside a boat building station volume, which the simulator doesn't have.
        return true;
      default:
        return false;
    }
  }

  /** DeployVolume.Check(position, rotation, OBB test, mask) */
  checkOBB(position: Vector3, rotation: Quaternion, test: OBB, mask = -1): boolean {
    const j = this.j;
    if ((this.layers & mask) === 0) return false;
    switch (this.kind) {
      case 'DeployVolumeOBB': {
        const b = bounds(j.bounds);
        const pos = position.add(rotation.rotate(this.worldRotation.rotate(b.center).add(this.worldPosition)));
        return OBB.fromSize(pos, b.size, rotation.mul(this.worldRotation)).intersects(test);
      }
      case 'DeployVolumeSphere': {
        const pos = position.add(rotation.rotate(this.worldRotation.rotate(vec(j.center)).add(this.worldPosition)));
        return Vector3.distance(pos, test.closestPoint(pos)) <= (j.radius ?? 0.5);
      }
      default:
        return false;
    }
  }

  /** DeployVolume.CheckFlags */
  private checkColliders(list: Collider[]): boolean {
    this.lastHit = null;
    for (const c of list) {
      this.lastHit = c;
      if (c.tag === 'DeployVolumeIgnore') continue;
      const flags = c.flags;
      const hasInfo = c.opts.flags !== undefined;
      if (hasInfo && (flags & ColliderFlags.OnlyBlockBuildingBlock) === ColliderFlags.OnlyBlockBuildingBlock && !this.isBuildingBlock) continue;
      if (hasInfo && (flags & ColliderFlags.OnlyBlockDeployables) === ColliderFlags.OnlyBlockDeployables && this.isBuildingBlock) continue;
      if (c.hasCustomTag('BlockPlacement')) return true;
      // No monuments in the simulator: the monument branches reduce to these two conditions.
      const notOnlyMonumentPrevent = (this.layers & 0x20000000) === 0 || (this.ignore & ColliderFlags.OnlyEvaluatePreventBuildingInMonuments) !== ColliderFlags.OnlyEvaluatePreventBuildingInMonuments;
      if (notOnlyMonumentPrevent && (!hasInfo || (this.ignore & flags) === 0)) {
        if (hasInfo && this.ignore !== 0 && (flags & this.ignore) === this.ignore) return false;
        if (shouldApplyVolumeForEntity(this, c.entity)) return true;
      }
    }
    return false;
  }
}

/** DeployVolume.ShouldApplyVolumeForEntity */
export function shouldApplyVolumeForEntity(v: DeployVolume, e: SimEntity | null): boolean {
  if (v.entityList.length === 0 && v.entityGroups.length === 0) return true;
  const include = v.entityMode === 1;
  for (const g of v.entityGroups) {
    const ents: J[] = g?.entities ?? [];
    if (ents.length && checkEntityList(e, ents, include)) return true;
  }
  if (v.entityList.length && checkEntityList(e, v.entityList, include)) return true;
  return false;
}

/** DeployVolume.Check(position, rotation, volumes) — true if any volume is blocked. */
export function checkVolumes(srv: VolumeServer, position: Vector3, rotation: Quaternion, volumes: DeployVolume[], mask = -1): DeployVolume | null {
  for (const v of volumes) if (v.check(srv, position, rotation, mask)) return v;
  return null;
}
