import { describe, expect, it } from 'vitest';
import type { PrefabDef } from '../src/core/entity';
import { ConstructionSocket, ConstructionSocketType } from '../src/core/sockets';
import { Bounds, Pose, Quaternion, Vector3 } from '../src/core/unity';
import { World } from '../src/core/world';

// Synthetic prefabs: real socket data comes from the server dump.
function socket(name: string, pos: Vector3, male: boolean, female: boolean, support = 1, extra: Partial<ConstructionSocket> = {}) {
  const s = new ConstructionSocket();
  Object.assign(s, { socketName: name, male, female, support, socketType: ConstructionSocketType.Wall, worldPosition: pos }, extra);
  return s;
}

const foundation: PrefabDef = {
  prefabID: 1, path: 'foundation', isStabilityEntity: true, grounded: true,
  bounds: new Bounds(new Vector3(0, -0.5, 0), new Vector3(3, 1, 3)),
  sockets: [socket('wall-female', new Vector3(0, 0, 1.5), false, true)],
};

function wallPrefab(support: number, extra: Partial<ConstructionSocket> = {}): PrefabDef {
  return {
    prefabID: 2, path: 'wall', isStabilityEntity: true, grounded: false,
    bounds: new Bounds(new Vector3(0, 1.5, 0), new Vector3(3, 3, 0.2)),
    sockets: [
      socket('wall-male', Vector3.zero, true, false, support),
      socket('wall-female', new Vector3(0, 3, 0), false, true, 1, extra),
    ],
  };
}

const at = (x: number, y: number, z: number) => new Pose(new Vector3(x, y, z), Quaternion.identity);

describe('stability', () => {
  it('wall on foundation is fully supported', () => {
    const w = new World();
    const f = w.spawn(foundation, at(0, 0, 0));
    const wall = w.spawn(wallPrefab(1), at(0, 0, 1.5));
    w.settle();
    expect(f.cachedStability).toBe(1);
    expect(wall.cachedStability).toBe(1);
    expect(wall.cachedDistanceFromGround).toBe(2);
  });

  it('stacked walls multiply support factor', () => {
    const w = new World();
    w.spawn(foundation, at(0, 0, 0));
    const p = wallPrefab(0.8);
    const w1 = w.spawn(p, at(0, 0, 1.5));
    const w2 = w.spawn(p, at(0, 3, 1.5));
    const w3 = w.spawn(p, at(0, 6, 1.5));
    w.settle();
    expect(w1.cachedStability).toBeCloseTo(0.8, 6);
    expect(w2.cachedStability).toBeCloseTo(0.64, 6);
    expect(w3.cachedStability).toBeCloseTo(0.512, 6);
    expect(w3.cachedDistanceFromGround).toBe(4);
  });

  it('removing the foundation collapses what it held', () => {
    const w = new World();
    const f = w.spawn(foundation, at(0, 0, 0));
    const p = wallPrefab(1);
    const w1 = w.spawn(p, at(0, 0, 1.5));
    const w2 = w.spawn(p, at(0, 3, 1.5));
    w.settle();
    w.kill(f);
    w.settle();
    expect(w1.destroyed).toBe(true);
    expect(w2.destroyed).toBe(true);
    expect(w.entities.length).toBe(0);
  });

  it('femaleNoStability sockets give no support', () => {
    const w = new World();
    w.spawn(foundation, at(0, 0, 0));
    const w1 = w.spawn(wallPrefab(1, { femaleNoStability: true }), at(0, 0, 1.5));
    const w2 = w.spawn(wallPrefab(1), at(0, 3, 1.5));
    w.settle();
    expect(w1.cachedStability).toBe(1);
    expect(w2.destroyed).toBe(true);
  });

  it('sockets must coincide within 0.02 m to link', () => {
    const w = new World();
    w.spawn(foundation, at(0, 0, 0));
    const near = w.spawn(wallPrefab(1), at(0.015, 0, 1.5));
    w.settle();
    expect(near.cachedStability).toBe(1);
    const w2 = new World();
    w2.spawn(foundation, at(0, 0, 0));
    const far = w2.spawn(wallPrefab(1), at(0.025, 0, 1.5));
    w2.settle();
    expect(far.destroyed).toBe(true);
  });

  it('collapse waits for the configured number of strikes', () => {
    // 10 strikes (ConVar stability.strikes) are spent on checks 1..10, the 11th check kills.
    const w = new World({ stabilityQueueBudget: 1 });
    const floating = w.spawn(wallPrefab(1), at(50, 0, 50));
    for (let i = 0; i < 10; i++) {
      w.cycle();
      expect(floating.destroyed).toBe(false);
    }
    w.cycle();
    expect(floating.destroyed).toBe(true);
  });
});
