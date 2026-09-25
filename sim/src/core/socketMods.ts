// SocketMod base (Assembly-CSharp/SocketMod.cs). Concrete mods are ported alongside the collision backend.
import type { Placement } from './construction';
import type { PlacementContext, SocketBase } from './sockets';

export interface SocketMod {
  readonly kind: string;
  baseSocket: SocketBase | null;
  socketGrouping: unknown | null;
  modifyPlacement(placement: Placement, ctx: PlacementContext): void;
  doCheck(placement: Placement, ctx: PlacementContext): boolean;
  errorMessage(): string;
}
