import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { describe, expect, it } from 'vitest';
import { GameData } from '../src/core/gamedata';
import { OBB } from '../src/core/obb';
import { Collider, PhysicsWorld, colliderShape, shapesOverlap } from '../src/core/physics';
import { Pose, Quaternion, Ray, Vector3 } from '../src/core/unity';

const json = JSON.parse(readFileSync(join(__dirname, '../public/data/game.json'), 'utf8'));
const bin = readFileSync(join(__dirname, '../public/data/meshes.bin'));
const data = new GameData(json);
data.meshBuffer = bin.buffer.slice(bin.byteOffset, bin.byteOffset + bin.byteLength);
const mesh = (id: string) => data.mesh(id);

function foundationWood(pose = new Pose()) {
  const skin = data.get('building core/foundation/foundation.wood');
  const w = new PhysicsWorld();
  for (const c of skin.colliders) {
    const s = colliderShape(c, pose, mesh);
    if (s) w.add(new Collider(s, c.layer, null, { name: c.node }));
  }
  return w;
}

describe('primitive overlaps', () => {
  const box = { kind: 'box' as const, obb: OBB.fromSize(Vector3.zero, Vector3.one, Quaternion.identity) };
  it('box vs sphere', () => {
    expect(shapesOverlap(box, { kind: 'sphere', c: new Vector3(1.4, 0, 0), r: 0.95 })).toBe(true);
    expect(shapesOverlap(box, { kind: 'sphere', c: new Vector3(1.4, 0, 0), r: 0.85 })).toBe(false);
  });
  it('box vs capsule', () => {
    const cap = (x: number) => ({ kind: 'capsule' as const, a: new Vector3(x, -3, 0), b: new Vector3(x, 3, 0), r: 0.3 });
    expect(shapesOverlap(box, cap(0.75))).toBe(true);
    expect(shapesOverlap(box, cap(0.85))).toBe(false);
  });
  it('capsule vs capsule, sphere vs sphere', () => {
    expect(shapesOverlap({ kind: 'capsule', a: Vector3.zero, b: Vector3.up, r: 0.5 }, { kind: 'capsule', a: new Vector3(0.9, 0.5, 0), b: new Vector3(0.9, 0.5, 2), r: 0.5 })).toBe(true);
    expect(shapesOverlap({ kind: 'sphere', c: Vector3.zero, r: 1 }, { kind: 'sphere', c: new Vector3(2.1, 0, 0), r: 1 })).toBe(false);
  });
  it('terrain half-space', () => {
    expect(shapesOverlap({ kind: 'sphere', c: new Vector3(0, 0.4, 0), r: 0.5 }, { kind: 'terrain', height: 0 })).toBe(true);
    expect(shapesOverlap({ kind: 'sphere', c: new Vector3(0, 0.6, 0), r: 0.5 }, { kind: 'terrain', height: 0 })).toBe(false);
  });
});

describe('mesh colliders from the dump', () => {
  it('ray from above hits the foundation top face (winding / back-face convention)', () => {
    const w = foundationWood();
    const hit = w.raycast(new Ray(new Vector3(0.3, 5, 0.2), Vector3.down));
    expect(hit).not.toBeNull();
    expect(hit!.point.y).toBeCloseTo(0.1, 4); // foundation surface is 0.1 above its origin
    expect(hit!.normal.y).toBeGreaterThan(0.9);
  });
  it('ray from below the foundation (inside the hollow mesh) does not hit the top from behind', () => {
    const w = foundationWood();
    // Just under the top face (the mesh has an inner lip at y≈-0.22), looking up.
    const hit = w.raycast(new Ray(new Vector3(0.3, 0.05, 0.2), Vector3.up), 2);
    expect(hit).toBeNull();
  });
  it('box touching the top surface overlaps, box floating above does not', () => {
    const w = foundationWood();
    expect(w.checkOBB(OBB.fromSize(new Vector3(0, 0.15, 0), new Vector3(1, 0.2, 1), Quaternion.identity))).toBe(true);
    expect(w.checkOBB(OBB.fromSize(new Vector3(0, 0.3, 0), new Vector3(1, 0.1, 1), Quaternion.identity))).toBe(false);
  });
  it('a small box fully inside the non-convex mesh does not overlap (no volume)', () => {
    const w = foundationWood();
    expect(w.checkOBB(OBB.fromSize(new Vector3(0, -1.5, 0), new Vector3(0.2, 0.2, 0.2), Quaternion.identity))).toBe(false);
  });
});
