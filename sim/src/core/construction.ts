// Construction.Target / Construction.Placement (Assembly-CSharp/Construction.cs).
import type { SimEntity } from './entity';
import type { SocketBase } from './sockets';
import type { Pose, Quaternion, Ray, Vector3 } from './unity';

export interface Target {
  valid: boolean;
  ray: Ray;
  entity: SimEntity | null;
  socket: SocketBase | null;
  onTerrain: boolean;
  position: Vector3;
  normal: Vector3;
  /** Euler degrees (player-applied rotation). */
  rotation: Vector3;
  /** target.player.eyes.position */
  playerEyes: Vector3;
  buildingBlocked: boolean;
  isHoldingShift: boolean;
  shouldParent: boolean;
}

export interface Placement {
  position: Vector3;
  rotation: Quaternion;
  isPopulated: boolean;
  shouldParent: boolean;
  parentPassed: boolean;
  isHoldingShift: boolean;
  transform: Pose | null;
  ignoredEntity: SimEntity | null;
}
