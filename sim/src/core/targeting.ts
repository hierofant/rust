// Client-side target selection (the Rust client's planner guide is IL2CPP and not in the
// server build). Chooses what the server's Construction.UpdatePlacement is asked to validate:
//   1. the nearest female socket whose select box (Socket_Base.GetSelectBounds) the view ray
//      crosses and that some male socket of the held construction accepts;
//   2. otherwise the surface under the crosshair (onTerrain = free placement).
import type { Target } from './construction';
import type { SimEntity } from './entity';
import type { LoadedPrefab } from './gamedata';
import { QueryTriggerInteraction } from './physics';
import type { BuildServer } from './server';
import type { SocketBase } from './sockets';
import { Ray, Vector3 } from './unity';

/** Layers the crosshair collides with: Default, Deployed, World, Construction, Terrain, Vehicle Large. */
export const AIM_MASK = (1 << 0) | (1 << 8) | (1 << 16) | (1 << 21) | (1 << 23) | (1 << 27);

export interface AimOptions {
  /** Player-applied rotation (Euler degrees), what R / scroll changes in game. */
  rotation?: Vector3;
  isHoldingShift?: boolean;
  /** Max aim distance; defaults to construction.maxplaceDistance + 1. */
  maxDistance?: number;
}

export interface SocketCandidate {
  entity: SimEntity;
  socket: SocketBase;
  distance: number;
  point: Vector3;
}

export function socketCandidates(server: BuildServer, prefab: LoadedPrefab, ray: Ray, maxDistance: number): SocketCandidate[] {
  const out: SocketCandidate[] = [];
  const males = prefab.sockets.filter((s) => s.male && !s.maleDummy);
  const mid = ray.at(maxDistance * 0.5);
  for (const e of server.visEntities(mid, maxDistance * 0.5 + 3, -1, QueryTriggerInteraction.Collide)) {
    for (const s of e.prefab.sockets) {
      if (!s.female || s.femaleDummy) continue;
      const probe: Target = baseTarget(ray, e, s, ray.origin, Vector3.up);
      if (!males.some((m) => m.testTarget(probe))) continue;
      if (server.isOccupied(e, s)) continue;
      const obb = s.getSelectBounds(e.pose.position, e.pose.rotation);
      const hit = obb.contains(ray.origin) ? { distance: 0, point: ray.origin } : obb.trace(ray, maxDistance);
      if (hit) out.push({ entity: e, socket: s, distance: hit.distance, point: hit.point });
    }
  }
  return out.sort((a, b) => a.distance - b.distance);
}

function baseTarget(ray: Ray, entity: SimEntity | null, socket: SocketBase | null, position: Vector3, normal: Vector3): Target {
  return {
    valid: true,
    ray,
    entity,
    socket,
    onTerrain: socket == null,
    position,
    normal,
    rotation: Vector3.zero,
    playerEyes: ray.origin,
    buildingBlocked: false,
    isHoldingShift: false,
    shouldParent: false,
  };
}

/** Builds the Construction.Target the client would send for this aim. */
export function aimTarget(server: BuildServer, prefab: LoadedPrefab, ray: Ray, opts: AimOptions = {}): Target {
  const con = prefab.construction!;
  const maxDistance = opts.maxDistance ?? con.maxplaceDistance + 1;
  const surface = server.physics.raycast(ray, maxDistance, AIM_MASK, QueryTriggerInteraction.Ignore);
  const candidates = socketCandidates(server, prefab, ray, maxDistance);
  // Sockets behind the surface under the crosshair are not selectable.
  const best = candidates.find((c) => !surface || c.distance <= surface.distance + 0.05);
  let t: Target;
  if (best) {
    t = baseTarget(ray, best.entity, best.socket, surface ? surface.point : best.point, surface ? surface.normal : Vector3.up);
  } else if (surface) {
    t = baseTarget(ray, surface.collider.entity, null, surface.point, surface.normal);
  } else {
    t = baseTarget(ray, null, null, ray.at(maxDistance), Vector3.up);
    t.valid = false;
  }
  t.rotation = opts.rotation ?? Vector3.zero;
  t.isHoldingShift = !!opts.isHoldingShift;
  return t;
}
