// Minimal ports of the UnityEngine math types Rust's building code relies on.
// Semantics follow Unity (left-handed, Y up, Euler order ZXY, degrees), not three.js.
// Arithmetic is float64; Unity is float32. Thresholds in the game code (0.02 m, 2°, 1.49 m)
// are far above float32 noise, so this is not expected to change outcomes.

export const Deg2Rad = Math.PI / 180;
export const Rad2Deg = 57.29578;
/** Mathf.Epsilon (smallest positive float32 denormal). */
export const FloatEpsilon = 1.401298e-45;
/** Vector3f::epsilon in Unity's native math. */
const NativeEpsilon = 0.00001;

export const clamp = (v: number, min: number, max: number) => (v < min ? min : v > max ? max : v);
export const clamp01 = (v: number) => clamp(v, 0, 1);

export class Vector3 {
  constructor(public readonly x = 0, public readonly y = 0, public readonly z = 0) {}

  static readonly zero = new Vector3(0, 0, 0);
  static readonly one = new Vector3(1, 1, 1);
  static readonly up = new Vector3(0, 1, 0);
  static readonly down = new Vector3(0, -1, 0);
  static readonly right = new Vector3(1, 0, 0);
  static readonly left = new Vector3(-1, 0, 0);
  static readonly forward = new Vector3(0, 0, 1);
  static readonly back = new Vector3(0, 0, -1);

  static from(a: ArrayLike<number> | { x: number; y: number; z: number }): Vector3 {
    if ('x' in a) return new Vector3(a.x, a.y, a.z);
    return new Vector3(a[0], a[1], a[2]);
  }

  add(b: Vector3) { return new Vector3(this.x + b.x, this.y + b.y, this.z + b.z); }
  sub(b: Vector3) { return new Vector3(this.x - b.x, this.y - b.y, this.z - b.z); }
  mul(s: number) { return new Vector3(this.x * s, this.y * s, this.z * s); }
  neg() { return new Vector3(-this.x, -this.y, -this.z); }
  scale(b: Vector3) { return new Vector3(this.x * b.x, this.y * b.y, this.z * b.z); }
  withY(y: number) { return new Vector3(this.x, y, this.z); }
  /** Vector3Ex.XZ3D */
  xz3d() { return new Vector3(this.x, 0, this.z); }

  get sqrMagnitude() { return this.x * this.x + this.y * this.y + this.z * this.z; }
  get magnitude() { return Math.sqrt(this.sqrMagnitude); }
  /** Vector3Ex.Magnitude2D (XZ plane) */
  magnitude2D() { return Math.sqrt(this.x * this.x + this.z * this.z); }

  get normalized(): Vector3 {
    const m = this.magnitude;
    return m > 1e-5 ? this.mul(1 / m) : Vector3.zero;
  }

  equals(b: Vector3) {
    // Unity's == : squared distance below 1e-10
    return this.sub(b).sqrMagnitude < 9.99999944e-11;
  }

  static dot(a: Vector3, b: Vector3) { return a.x * b.x + a.y * b.y + a.z * b.z; }
  static cross(a: Vector3, b: Vector3) {
    return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
  }
  static distance(a: Vector3, b: Vector3) { return a.sub(b).magnitude; }

  /** Vector3.Angle, degrees. */
  static angle(from: Vector3, to: Vector3) {
    const num = Math.sqrt(from.sqrMagnitude * to.sqrMagnitude);
    if (num < 1e-15) return 0;
    return Math.acos(clamp(Vector3.dot(from, to) / num, -1, 1)) * Rad2Deg;
  }

  /** Vector3Ex.DotDegrees */
  static dotDegrees(a: Vector3, b: Vector3) {
    return Math.acos(clamp(Vector3.dot(a.normalized, b.normalized), -1, 1)) * Rad2Deg;
  }

  static lerp(a: Vector3, b: Vector3, t: number) {
    t = clamp01(t);
    return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
  }

  static projectOnPlane(v: Vector3, n: Vector3) {
    const sq = n.sqrMagnitude;
    if (sq < FloatEpsilon) return v;
    return v.sub(n.mul(Vector3.dot(v, n) / sq));
  }

  static min(a: Vector3, b: Vector3) { return new Vector3(Math.min(a.x, b.x), Math.min(a.y, b.y), Math.min(a.z, b.z)); }
  static max(a: Vector3, b: Vector3) { return new Vector3(Math.max(a.x, b.x), Math.max(a.y, b.y), Math.max(a.z, b.z)); }

  toString() { return `(${this.x.toFixed(3)}, ${this.y.toFixed(3)}, ${this.z.toFixed(3)})`; }
}

export class Quaternion {
  constructor(public readonly x = 0, public readonly y = 0, public readonly z = 0, public readonly w = 1) {}

  static readonly identity = new Quaternion(0, 0, 0, 1);

  static from(a: ArrayLike<number> | { x: number; y: number; z: number; w: number }): Quaternion {
    if ('w' in a) return new Quaternion(a.x, a.y, a.z, a.w);
    return new Quaternion(a[0], a[1], a[2], a[3]);
  }

  /** Quaternion * Quaternion */
  mul(r: Quaternion): Quaternion {
    const l = this;
    return new Quaternion(
      l.w * r.x + l.x * r.w + l.y * r.z - l.z * r.y,
      l.w * r.y + l.y * r.w + l.z * r.x - l.x * r.z,
      l.w * r.z + l.z * r.w + l.x * r.y - l.y * r.x,
      l.w * r.w - l.x * r.x - l.y * r.y - l.z * r.z,
    );
  }

  /** Quaternion * Vector3 (Unity's formula). */
  rotate(p: Vector3): Vector3 {
    const num = this.x * 2, num2 = this.y * 2, num3 = this.z * 2;
    const num4 = this.x * num, num5 = this.y * num2, num6 = this.z * num3;
    const num7 = this.x * num2, num8 = this.x * num3, num9 = this.y * num3;
    const num10 = this.w * num, num11 = this.w * num2, num12 = this.w * num3;
    return new Vector3(
      (1 - (num5 + num6)) * p.x + (num7 - num12) * p.y + (num8 + num11) * p.z,
      (num7 + num12) * p.x + (1 - (num4 + num6)) * p.y + (num9 - num10) * p.z,
      (num8 - num11) * p.x + (num9 + num10) * p.y + (1 - (num4 + num5)) * p.z,
    );
  }

  /** Quaternion.Inverse (Unity returns the conjugate). */
  inverse() { return new Quaternion(-this.x, -this.y, -this.z, this.w); }

  static angleAxis(angleDeg: number, axis: Vector3): Quaternion {
    const m = axis.magnitude;
    if (m < 1e-6) return Quaternion.identity;
    const a = axis.mul(1 / m);
    const h = angleDeg * Deg2Rad * 0.5;
    const s = Math.sin(h);
    return new Quaternion(a.x * s, a.y * s, a.z * s, Math.cos(h));
  }

  /** Quaternion.Euler: rotates Z, then X, then Y. */
  static euler(x: number, y: number, z: number): Quaternion;
  static euler(v: Vector3): Quaternion;
  static euler(a: number | Vector3, b?: number, c?: number): Quaternion {
    const [x, y, z] = a instanceof Vector3 ? [a.x, a.y, a.z] : [a, b!, c!];
    const hx = x * Deg2Rad * 0.5, hy = y * Deg2Rad * 0.5, hz = z * Deg2Rad * 0.5;
    const cx = Math.cos(hx), sx = Math.sin(hx);
    const cy = Math.cos(hy), sy = Math.sin(hy);
    const cz = Math.cos(hz), sz = Math.sin(hz);
    // qY * qX * qZ
    return new Quaternion(
      cy * sx * cz + sy * cx * sz,
      sy * cx * cz - cy * sx * sz,
      cy * cx * sz - sy * sx * cz,
      cy * cx * cz + sy * sx * sz,
    );
  }

  /** Quaternion.LookRotation including Unity's fallback for forward parallel to up. */
  static lookRotation(forward: Vector3, up: Vector3 = Vector3.up): Quaternion {
    const fm = forward.magnitude;
    if (fm < NativeEpsilon) return Quaternion.identity;
    const z = forward.mul(1 / fm);
    let x = Vector3.cross(up, z);
    const xm = x.magnitude;
    if (xm < NativeEpsilon) return Quaternion.fromToRotation(Vector3.forward, z);
    x = x.mul(1 / xm);
    const y = Vector3.cross(z, x);
    return Quaternion.fromBasis(x, y, z);
  }

  static fromToRotation(from: Vector3, to: Vector3): Quaternion {
    const f = from.normalized, t = to.normalized;
    const d = Vector3.dot(f, t);
    if (d >= 1 - 1e-12) return Quaternion.identity;
    if (d <= -1 + 1e-12) {
      let axis = Vector3.cross(Vector3.right, f);
      if (axis.sqrMagnitude < 1e-12) axis = Vector3.cross(Vector3.up, f);
      return Quaternion.angleAxis(180, axis);
    }
    const c = Vector3.cross(f, t);
    const q = new Quaternion(c.x, c.y, c.z, 1 + d);
    return q.normalized();
  }

  /** Rotation whose columns are the given orthonormal axes. */
  static fromBasis(x: Vector3, y: Vector3, z: Vector3): Quaternion {
    const m00 = x.x, m01 = y.x, m02 = z.x;
    const m10 = x.y, m11 = y.y, m12 = z.y;
    const m20 = x.z, m21 = y.z, m22 = z.z;
    const tr = m00 + m11 + m22;
    if (tr > 0) {
      const s = Math.sqrt(tr + 1) * 2;
      return new Quaternion((m21 - m12) / s, (m02 - m20) / s, (m10 - m01) / s, 0.25 * s);
    } else if (m00 > m11 && m00 > m22) {
      const s = Math.sqrt(1 + m00 - m11 - m22) * 2;
      return new Quaternion(0.25 * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s);
    } else if (m11 > m22) {
      const s = Math.sqrt(1 + m11 - m00 - m22) * 2;
      return new Quaternion((m01 + m10) / s, 0.25 * s, (m12 + m21) / s, (m02 - m20) / s);
    } else {
      const s = Math.sqrt(1 + m22 - m00 - m11) * 2;
      return new Quaternion((m02 + m20) / s, (m12 + m21) / s, 0.25 * s, (m10 - m01) / s);
    }
  }

  normalized(): Quaternion {
    const m = Math.hypot(this.x, this.y, this.z, this.w);
    if (m < FloatEpsilon) return Quaternion.identity;
    return new Quaternion(this.x / m, this.y / m, this.z / m, this.w / m);
  }

  static dot(a: Quaternion, b: Quaternion) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

  /** Quaternion.Angle, degrees. */
  static angle(a: Quaternion, b: Quaternion) {
    const d = Math.min(Math.abs(Quaternion.dot(a, b)), 1);
    return d > 1 - 1e-6 ? 0 : Math.acos(d) * 2 * Rad2Deg;
  }

  /** 3x3 rotation matrix, row-major m[row][col] (Matrix4x4.Rotate). */
  toMatrix3(): number[][] {
    const { x, y, z, w } = this;
    const xx = x * x, yy = y * y, zz = z * z, xy = x * y, xz = x * z, yz = y * z, wx = w * x, wy = w * y, wz = w * z;
    return [
      [1 - 2 * (yy + zz), 2 * (xy - wz), 2 * (xz + wy)],
      [2 * (xy + wz), 1 - 2 * (xx + zz), 2 * (yz - wx)],
      [2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xx + yy)],
    ];
  }

  get forward() { return this.rotate(Vector3.forward); }
  get up() { return this.rotate(Vector3.up); }
  get right() { return this.rotate(Vector3.right); }
}

/** UnityEngine.Bounds (center + size). */
export class Bounds {
  constructor(public readonly center: Vector3, public readonly size: Vector3) {}
  get extents() { return this.size.mul(0.5); }
  get min() { return this.center.sub(this.extents); }
  get max() { return this.center.add(this.extents); }
  static fromMinMax(min: Vector3, max: Vector3) {
    return new Bounds(min.add(max).mul(0.5), max.sub(min));
  }
  encapsulate(p: Vector3) {
    return Bounds.fromMinMax(Vector3.min(this.min, p), Vector3.max(this.max, p));
  }
}

/** Rigid transform (position + rotation, unit scale) — what building entities use. */
export class Pose {
  constructor(public readonly position: Vector3 = Vector3.zero, public readonly rotation: Quaternion = Quaternion.identity) {}
  /** Matrix4x4.TRS(p, r, one).MultiplyPoint3x4 */
  point(p: Vector3) { return this.position.add(this.rotation.rotate(p)); }
  /** MultiplyVector */
  vector(v: Vector3) { return this.rotation.rotate(v); }
  inversePoint(p: Vector3) { return this.rotation.inverse().rotate(p.sub(this.position)); }
}

export class Ray {
  readonly direction: Vector3;
  constructor(public readonly origin: Vector3, direction: Vector3) {
    this.direction = direction.normalized;
  }
  at(t: number) { return this.origin.add(this.direction.mul(t)); }
}
