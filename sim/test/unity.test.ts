import { describe, expect, it } from 'vitest';
import { OBB } from '../src/core/obb';
import { Quaternion, Ray, Vector3 } from '../src/core/unity';

const close = (a: Vector3, b: Vector3, eps = 1e-6) => {
  expect(Math.abs(a.x - b.x)).toBeLessThan(eps);
  expect(Math.abs(a.y - b.y)).toBeLessThan(eps);
  expect(Math.abs(a.z - b.z)).toBeLessThan(eps);
};

describe('Unity math conventions', () => {
  it('Euler yaw 90 turns forward into right (left-handed, Y up)', () => {
    close(Quaternion.euler(0, 90, 0).rotate(Vector3.forward), Vector3.right);
  });
  it('Euler pitch 90 turns forward down', () => {
    close(Quaternion.euler(90, 0, 0).rotate(Vector3.forward), Vector3.down);
  });
  it('Euler roll 90 turns right into up', () => {
    close(Quaternion.euler(0, 0, 90).rotate(Vector3.right), Vector3.up);
  });
  it('Euler applies Z, then X, then Y', () => {
    const q = Quaternion.euler(30, 40, 50);
    const manual = Quaternion.angleAxis(40, Vector3.up).mul(Quaternion.angleAxis(30, Vector3.right)).mul(Quaternion.angleAxis(50, Vector3.forward));
    close(q.rotate(new Vector3(1, 2, 3)), manual.rotate(new Vector3(1, 2, 3)));
  });
  it('LookRotation(right) equals yaw 90', () => {
    close(Quaternion.lookRotation(Vector3.right).rotate(new Vector3(1, 2, 3)), Quaternion.euler(0, 90, 0).rotate(new Vector3(1, 2, 3)));
  });
  it('LookRotation with forward parallel to up falls back to FromToRotation', () => {
    close(Quaternion.lookRotation(Vector3.up, Vector3.up).rotate(Vector3.forward), Vector3.up);
  });
  it('Vector3.Angle', () => {
    expect(Vector3.angle(Vector3.forward, Vector3.right)).toBeCloseTo(90, 4);
    expect(Vector3.angle(Vector3.zero, Vector3.right)).toBe(0);
  });
  it('inverse undoes rotation', () => {
    const q = Quaternion.euler(10, 20, 30);
    close(q.inverse().rotate(q.rotate(new Vector3(4, 5, 6))), new Vector3(4, 5, 6));
  });
});

describe('OBB', () => {
  const unit = (p: Vector3, yaw = 0) => OBB.fromSize(p, Vector3.one, Quaternion.euler(0, yaw, 0));
  it('intersects overlapping boxes', () => {
    expect(unit(Vector3.zero).intersects(unit(new Vector3(0.9, 0, 0)))).toBe(true);
  });
  it('separates distant boxes', () => {
    expect(unit(Vector3.zero).intersects(unit(new Vector3(1.1, 0, 0)))).toBe(false);
  });
  it('rotated box reaches further along its diagonal', () => {
    // 45° box has half-diagonal 0.707 along X
    expect(unit(Vector3.zero).intersects(unit(new Vector3(1.15, 0, 0), 45))).toBe(true);
    expect(unit(Vector3.zero).intersects(unit(new Vector3(1.25, 0, 0), 45))).toBe(false);
  });
  it('trace hits the near face', () => {
    const hit = unit(Vector3.zero).trace(new Ray(new Vector3(-5, 0, 0), Vector3.right));
    expect(hit?.distance).toBeCloseTo(4.5, 6);
  });
  it('closest point / contains', () => {
    const b = unit(Vector3.zero);
    expect(b.contains(new Vector3(0.2, 0.2, 0.2))).toBe(true);
    expect(b.contains(new Vector3(0.6, 0, 0))).toBe(false);
    close(b.closestPoint(new Vector3(3, 0, 0)), new Vector3(0.5, 0, 0));
  });
});
