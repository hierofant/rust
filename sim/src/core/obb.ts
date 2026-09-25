// Port of Rust.Global/OBB.cs
import { Bounds, FloatEpsilon, Quaternion, Ray, Vector3, clamp } from './unity';

export interface RaycastHit {
  point: Vector3;
  distance: number;
}

export class OBB {
  readonly forward: Vector3;
  readonly right: Vector3;
  readonly up: Vector3;
  readonly reject: number;

  constructor(
    public readonly position: Vector3,
    public readonly extents: Vector3,
    public readonly rotation: Quaternion,
  ) {
    this.forward = rotation.rotate(Vector3.forward);
    this.right = rotation.rotate(Vector3.right);
    this.up = rotation.rotate(Vector3.up);
    this.reject = extents.sqrMagnitude;
  }

  /** new OBB(position, scale, rotation, bounds) */
  static fromBounds(position: Vector3, rotation: Quaternion, bounds: Bounds, scale: Vector3 = Vector3.one): OBB {
    return new OBB(position.add(rotation.rotate(scale.scale(bounds.center))), scale.scale(bounds.extents), rotation);
  }

  /** new OBB(position, size, rotation) */
  static fromSize(position: Vector3, size: Vector3, rotation: Quaternion): OBB {
    return new OBB(position, size.mul(0.5), rotation);
  }

  getPoint(x: number, y: number, z: number): Vector3 {
    return this.position
      .add(this.right.mul(x * this.extents.x))
      .add(this.up.mul(y * this.extents.y))
      .add(this.forward.mul(z * this.extents.z));
  }

  corners(): Vector3[] {
    const out: Vector3[] = [];
    for (const x of [-1, 1]) for (const y of [-1, 1]) for (const z of [-1, 1]) out.push(this.getPoint(x, y, z));
    return out;
  }

  toBounds(): Bounds {
    let b = new Bounds(this.position, Vector3.zero);
    for (const c of this.corners()) b = b.encapsulate(c);
    return b;
  }

  contains(target: Vector3): boolean {
    if (target.sub(this.position).sqrMagnitude > this.reject) return false;
    return this.closestPoint(target).equals(target);
  }

  intersects(target: OBB): boolean {
    const a = this.rotation.toMatrix3();
    const b = target.rotation.toMatrix3();
    // vector = inverse(rotA) * (posB - posA)
    const d = target.position.sub(this.position);
    const vx = a[0][0] * d.x + a[1][0] * d.y + a[2][0] * d.z;
    const vy = a[0][1] * d.x + a[1][1] * d.y + a[2][1] * d.z;
    const vz = a[0][2] * d.x + a[1][2] * d.y + a[2][2] * d.z;
    // m = transpose(A) * B
    const m: number[][] = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    const id: number[][] = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    for (let i = 0; i < 3; i++)
      for (let j = 0; j < 3; j++) {
        m[i][j] = a[0][i] * b[0][j] + a[1][i] * b[1][j] + a[2][i] * b[2][j];
        id[i][j] = Math.abs(m[i][j]) + FloatEpsilon;
      }
    const e = this.extents, t = target.extents;
    const abs = Math.abs;
    if (abs(vx) > e.x + t.x * id[0][0] + t.y * id[0][1] + t.z * id[0][2]) return false;
    if (abs(vy) > e.y + t.x * id[1][0] + t.y * id[1][1] + t.z * id[1][2]) return false;
    if (abs(vz) > e.z + t.x * id[2][0] + t.y * id[2][1] + t.z * id[2][2]) return false;
    if (abs(vx * m[0][0] + vy * m[1][0] + vz * m[2][0]) > e.x * id[0][0] + e.y * id[1][0] + e.z * id[2][0] + t.x) return false;
    if (abs(vx * m[0][1] + vy * m[1][1] + vz * m[2][1]) > e.x * id[0][1] + e.y * id[1][1] + e.z * id[2][1] + t.y) return false;
    if (abs(vx * m[0][2] + vy * m[1][2] + vz * m[2][2]) > e.x * id[0][2] + e.y * id[1][2] + e.z * id[2][2] + t.z) return false;
    if (abs(vz * m[1][0] - vy * m[2][0]) > e.y * id[2][0] + e.z * id[1][0] + t.y * id[0][2] + t.z * id[0][1]) return false;
    if (abs(vz * m[1][1] - vy * m[2][1]) > e.y * id[2][1] + e.z * id[1][1] + t.x * id[0][2] + t.z * id[0][0]) return false;
    if (abs(vz * m[1][2] - vy * m[2][2]) > e.y * id[2][2] + e.z * id[1][2] + t.x * id[0][1] + t.y * id[0][0]) return false;
    if (abs(vx * m[2][0] - vz * m[0][0]) > e.x * id[2][0] + e.z * id[0][0] + t.y * id[1][2] + t.z * id[1][1]) return false;
    if (abs(vx * m[2][1] - vz * m[0][1]) > e.x * id[2][1] + e.z * id[0][1] + t.x * id[1][2] + t.z * id[1][0]) return false;
    if (abs(vx * m[2][2] - vz * m[0][2]) > e.x * id[2][2] + e.z * id[0][2] + t.x * id[1][1] + t.y * id[1][0]) return false;
    if (abs(vy * m[0][0] - vx * m[1][0]) > e.x * id[1][0] + e.y * id[0][0] + t.y * id[2][2] + t.z * id[2][1]) return false;
    if (abs(vy * m[0][1] - vx * m[1][1]) > e.x * id[1][1] + e.y * id[0][1] + t.x * id[2][2] + t.z * id[2][0]) return false;
    if (abs(vy * m[0][2] - vx * m[1][2]) > e.x * id[1][2] + e.y * id[0][2] + t.x * id[2][1] + t.y * id[2][0]) return false;
    return true;
  }

  trace(ray: Ray, maxDistance = Infinity): RaycastHit | null {
    const { x, y, z } = this.extents;
    const lhs = ray.origin.sub(this.position);
    const dir = ray.direction;
    const slab = (dd: number, ld: number, ext: number): [number, number] => {
      if (dd > 0) return [(-ext - ld) / dd, (ext - ld) / dd];
      if (dd < 0) return [(ext - ld) / dd, (-ext - ld) / dd];
      return [-3.4028235e38, 3.4028235e38];
    };
    const [f, f2] = slab(Vector3.dot(dir, this.right), Vector3.dot(lhs, this.right), x);
    const [f3, f4] = slab(Vector3.dot(dir, this.up), Vector3.dot(lhs, this.up), y);
    const [f5, f6] = slab(Vector3.dot(dir, this.forward), Vector3.dot(lhs, this.forward), z);
    const tMax = Math.min(f2, f4, f6);
    if (tMax < 0) return null;
    const tMin = Math.max(f, f3, f5);
    if (tMin > tMax) return null;
    // Mathf.Clamp(0, tMin, tMax) — note the argument order in the original.
    const t = clamp(0, tMin, tMax);
    if (t > maxDistance) return null;
    return { point: ray.at(t), distance: t };
  }

  closestPoint(target: Vector3): Vector3 {
    let inX = false, inY = false, inZ = false;
    let result = this.position;
    const lhs = target.sub(this.position);
    const axis = (n: number, ext: number, dir: Vector3) => {
      if (n > ext) result = result.add(dir.mul(ext));
      else if (n < -ext) result = result.sub(dir.mul(ext));
      else { result = result.add(dir.mul(n)); return true; }
      return false;
    };
    inX = axis(Vector3.dot(lhs, this.right), this.extents.x, this.right);
    inY = axis(Vector3.dot(lhs, this.up), this.extents.y, this.up);
    inZ = axis(Vector3.dot(lhs, this.forward), this.extents.z, this.forward);
    if (inX && inY && inZ) return target;
    return result;
  }
}
