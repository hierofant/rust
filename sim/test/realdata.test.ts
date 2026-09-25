import { describe, expect, it } from 'vitest';
import type { Target } from '../src/core/construction';
import { BuildServer } from '../src/core/server';
import { loadData } from './helpers';
import type { SimEntity } from '../src/core/entity';
import { noPhysics } from '../src/core/physics';
import type { SocketBase } from '../src/core/sockets';
import { Pose, Quaternion, Ray, Vector3 } from '../src/core/unity';

const data = loadData();
const P = {
  foundation: data.get('building core/foundation/foundation'),
  wall: data.get('building core/wall/wall'),
  floor: data.get('building core/floor/floor'),
};

function socketOf(e: SimEntity, name: string): SocketBase {
  const s = e.prefab.sockets.find((x) => x.socketName.endsWith(name));
  if (!s) throw new Error(`no socket ${name} on ${e.prefab.path}`);
  return s;
}

/** Places `prefab` on `female` socket of `entity` the way UpdatePlacement does (first male socket that fits). */
function placeOn(world: BuildServer, prefab: typeof P.wall, entity: SimEntity, female: string): SimEntity {
  const socket = socketOf(entity, female);
  const eye = entity.pose.point(socket.position).add(new Vector3(0, 1.5, -3));
  const aim = entity.pose.point(socket.position);
  const target: Target = {
    valid: true, ray: new Ray(eye, aim.sub(eye)), entity, socket, onTerrain: false,
    position: aim, normal: Vector3.up, rotation: Vector3.zero, playerEyes: eye,
    buildingBlocked: false, isHoldingShift: false, shouldParent: false,
  };
  const ctx = { lastPlacementError: '', world: noPhysics };
  for (const male of prefab.sockets) {
    if (!male.male || male.maleDummy || !male.testTarget(target)) continue;
    const pl = male.doPlacement(target, ctx);
    if (pl) return world.spawnPlaced(prefab, new Pose(pl.position, pl.rotation));
  }
  throw new Error(`cannot place ${prefab.path} on ${female}`);
}

describe('real prefab data', () => {
  it('loads every placeable with typed sockets', () => {
    expect(data.placeables.length).toBe(580);
    const kinds = new Set(data.placeables.flatMap((p) => p.sockets.map((s) => s.kind)));
    expect(kinds).toContain('ConstructionSocket');
    expect(kinds).toContain('Socket_Free');
  });

  it('wall snaps onto a foundation edge and links', () => {
    const w = new BuildServer(data);
    const f = w.spawnPlaced(P.foundation, new Pose());
    const wall = placeOn(w, P.wall, f, 'wall-female/1');
    w.settle();
    // wall-female/1 sits at z = -1.5 on the foundation top
    expect(wall.pose.position.z).toBeCloseTo(-1.5, 4);
    expect(wall.pose.position.y).toBeCloseTo(0, 4);
    const maleLink = wall.links.find((l) => l.socket.socketName.endsWith('wall-male'))!;
    expect(maleLink.connections.map((c) => c.owner)).toContain(f);
    expect(wall.cachedStability).toBeCloseTo(0.7, 5); // wall-male support = 0.7 in this build
  });

  it('walls stacked on walls lose 30% per level', () => {
    const w = new BuildServer(data);
    const f = w.spawnPlaced(P.foundation, new Pose());
    const w1 = placeOn(w, P.wall, f, 'wall-female/1');
    const w2 = placeOn(w, P.wall, w1, 'wall/sockets/wall-female');
    w.settle();
    expect(w2.pose.position.y).toBeCloseTo(3, 4);
    expect(w2.cachedStability).toBeCloseTo(0.49, 5);
  });

  it('floor on four walls around a foundation', () => {
    const w = new BuildServer(data);
    const f = w.spawnPlaced(P.foundation, new Pose());
    const walls = [1, 2, 3, 4].map((i) => placeOn(w, P.wall, f, `wall-female/${i}`));
    w.settle();
    // floor-female/1 is the outer side of the wall, /2 the inner one.
    const floor = placeOn(w, P.floor, walls[0], 'floor-female/2');
    w.settle();
    expect(floor.pose.position.x).toBeCloseTo(0, 4);
    expect(floor.pose.position.y).toBeCloseTo(3, 4);
    expect(floor.pose.position.z).toBeCloseTo(0, 4);
    // Each floor-male socket (support 0.25) links to one of the four walls.
    const supports = floor.links.filter((l) => l.isMale() && l.connections.length > 0);
    expect(supports.length).toBe(4);
    // SupportValue reads each wall's support recomputed without the floor (CachedSupportValue),
    // not wall.cachedStability, so the floor lands near but not exactly at 4 * 0.25 * wall.
    expect(floor.cachedStability).toBeGreaterThan(0.85);
    expect(floor.cachedStability).toBeLessThan(0.88);
  });
});
