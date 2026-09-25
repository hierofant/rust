import { describe, expect, it } from 'vitest';
import type { SimEntity } from '../src/core/entity';
import { BuildServer } from '../src/core/server';
import { aimTarget } from '../src/core/targeting';
import { Pose, Quaternion, Ray, Vector3 } from '../src/core/unity';
import { loadData } from './helpers';

const data = loadData();
const P = {
  foundation: data.get('building core/foundation/foundation'),
  wall: data.get('building core/wall/wall'),
  floor: data.get('building core/floor/floor'),
  box: data.get('deployable/large wood storage/box.wooden.large'),
};

/** Stand at `feet`, look at `at`, try to build. */
function buildAt(s: BuildServer, prefab: typeof P.wall, feet: Vector3, at: Vector3, rotation = Vector3.zero) {
  const eyes = feet.add(new Vector3(0, 1.5, 0));
  s.setPlayer({ position: feet, eyes });
  const t = aimTarget(s, prefab, new Ray(eyes, at.sub(eyes)), { rotation });
  const r = s.build(prefab, t);
  s.settle();
  return r;
}

describe('BuildServer placement', () => {
  it('foundation on terrain, then walls on its edges', () => {
    const s = new BuildServer(data);
    const f = buildAt(s, P.foundation, new Vector3(0, 0, -4), new Vector3(0, 0, 0));
    expect(f.error).toBe('');
    const fe = f.entity!;
    // foundation sits on the terrain: its top (origin) roughly at ground level
    expect(fe.pose.position.y).toBeGreaterThan(-0.5);
    const w = buildAt(s, P.wall, fe.pose.position.add(new Vector3(0, 0, -4)), fe.pose.point(new Vector3(0, 1, -1.5)));
    expect(w.error).toBe('');
    expect(w.entity!.cachedStability).toBeCloseTo(0.7, 3);
  });

  it('second foundation snaps next to the first', () => {
    const s = new BuildServer(data);
    const f = buildAt(s, P.foundation, new Vector3(0, 0, -4), new Vector3(0, 0, 0)).entity!;
    const f2 = buildAt(s, P.foundation, f.pose.point(new Vector3(-4.5, 0, 0)), f.pose.point(new Vector3(-1.6, 0, 0)));
    expect(f2.error).toBe('');
    expect(Vector3.distance(f2.entity!.pose.position, f.pose.point(new Vector3(-3, 0, 0)))).toBeLessThan(0.01);
  });

  it('foundation overlapping another is blocked by its deploy volume', () => {
    const s = new BuildServer(data);
    const f = s.spawnPlaced(P.foundation, new Pose(new Vector3(0, 0.1, 0), Quaternion.identity));
    s.settle();
    const eyes = new Vector3(1, 1.6, -4);
    s.setPlayer({ position: new Vector3(1, 0.1, -4), eyes });
    const t = aimTarget(s, P.foundation, new Ray(eyes, new Vector3(1, 0, 0).sub(eyes)));
    const r = s.updatePlacement(P.foundation, { ...t, entity: null, socket: null, onTerrain: true, position: new Vector3(1, 0, 0) });
    expect(r.ok).toBe(false);
    expect(f.destroyed).toBe(false);
  });

  it('large box on a foundation, second box on the same spot is blocked', () => {
    const s = new BuildServer(data);
    const f = buildAt(s, P.foundation, new Vector3(0, 0, -4), new Vector3(0, 0, 0)).entity!;
    const top = f.pose.position.add(new Vector3(0, 0.1, 0));
    const feet = f.pose.position.add(new Vector3(0, 0.1, -1.3));
    const b1 = buildAt(s, P.box, feet, top);
    expect(b1.error).toBe('');
    // Aiming at the same spot now hits the first box's side: Rust answers "Invalid angle" too.
    expect(buildAt(s, P.box, feet, top).entity).toBeNull();
    // Force a target on the foundation surface 0.3 m next to the first box.
    const eyes = feet.add(new Vector3(0, 1.5, 0));
    const pos = top.add(new Vector3(0.3, 0, 0));
    const t = aimTarget(s, P.box, new Ray(eyes, pos.sub(eyes)));
    const r = s.updatePlacement(P.box, { ...t, entity: f, socket: null, onTerrain: true, position: pos, normal: Vector3.up });
    expect(r.ok).toBe(false);
    expect(r.error).toMatch(/Blocked by|Not enough space/);
  });

  it('wall cannot stack right next to another wall (BuildingProximity)', () => {
    const s = new BuildServer(data);
    const f = s.spawnPlaced(P.foundation, new Pose(new Vector3(0, 0.1, 0), Quaternion.identity));
    s.settle();
    const w = s.spawnPlaced(P.wall, new Pose(f.pose.point(new Vector3(0, 0, -1.5)), f.pose.rotation.mul(Quaternion.euler(0, 90, 0))));
    s.settle();
    // A wall 0.5 m inside the first one, not on any socket.
    const err = s.buildingProximity(P.wall, w.pose.point(new Vector3(0.5, 0, 0)), w.pose.rotation);
    expect(err).toMatch(/Too close/);
    expect(s.buildingProximity(P.wall, w.pose.position, w.pose.rotation)).toBeNull(); // exactly on a socket pose: connection
  });

  it('upgrading swaps the skin colliders; demolishing the foundation collapses the wall', () => {
    const s = new BuildServer(data);
    const f = buildAt(s, P.foundation, new Vector3(0, 0, -4), new Vector3(0, 0, 0)).entity!;
    const w: SimEntity = buildAt(s, P.wall, f.pose.position.add(new Vector3(0, 0, -4)), f.pose.point(new Vector3(0, 1, -1.5))).entity!;
    expect(s.upgrade(w, 2, 0)).toBeNull();
    expect(s.data.prefabs.get(w.skinPrefab!)!.path).toContain('wall.stone');
    expect(s.upgrade(w, 1, 0)).toMatch(/downgrade/);
    s.demolish(f);
    s.settle();
    expect(w.destroyed).toBe(true);
  });
});
