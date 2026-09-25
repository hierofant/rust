// Physics queries the placement code needs (Physics.Raycast / OverlapOBB / ...).
// Implemented later on top of the dumped colliders; kept as an interface so the ported
// game logic does not depend on the collision backend.
import type { Vector3 } from './unity';

export interface PhysicsQueries {
  /** Physics.Raycast(origin, dir, maxDistance, layerMask) -> hit anything */
  raycastAny(origin: Vector3, direction: Vector3, maxDistance: number, layerMask: number): boolean;
}

export const noPhysics: PhysicsQueries = {
  raycastAny: () => false,
};
