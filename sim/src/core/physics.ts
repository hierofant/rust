// Collision world standing in for Unity/PhysX scene queries (Physics.OverlapBox, CheckSphere,
// Raycast, ...). Semantics followed:
//  - overlap queries report colliders whose surface or volume intersects the query shape;
//    non-convex MeshColliders have no volume (only triangles count), like PhysX triangle meshes;
//  - raycasts ignore colliders that contain the ray origin and back faces of meshes
//    (Physics.queriesHitBackfaces = false);
//  - triggers are included unless QueryTriggerInteraction.Ignore;
//  - terrain is a flat ground plane; anything at or below it overlaps.
import type { SimEntity } from './entity';
import type { ColliderJson } from './gamedata';
import { OBB } from './obb';
import { Pose, Quaternion, Ray, Vector3 } from './unity';

export enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
/** Physics.queriesHitTriggers (Unity default; Rust's project setting is not in the dump). */
export const queriesHitTriggers = true;

export interface PhysicsQueries {
  /** Physics.Raycast(origin, dir, maxDistance, layerMask) -> hit anything */
  raycastAny(origin: Vector3, direction: Vector3, maxDistance: number, layerMask: number): boolean;
}

export const noPhysics: PhysicsQueries = {
  raycastAny: () => false,
};

export type Shape =
  | { kind: 'box'; obb: OBB }
  | { kind: 'sphere'; c: Vector3; r: number }
  | { kind: 'capsule'; a: Vector3; b: Vector3; r: number }
  | { kind: 'mesh'; tris: Float64Array; convex: boolean }
  | { kind: 'terrain'; height: number };

interface AABB { min: Vector3; max: Vector3 }

export class Collider {
  private static nextId = 1;
  readonly id = Collider.nextId++;
  readonly aabb: AABB;
  enabled = true;

  constructor(
    readonly shape: Shape,
    readonly layer: number,
    readonly entity: SimEntity | null,
    readonly opts: { isTrigger?: boolean; flags?: number; tag?: string; customTags?: string[]; name?: string } = {},
  ) {
    this.aabb = shapeAABB(shape);
  }
  get isTrigger() { return !!this.opts.isTrigger; }
  get flags() { return this.opts.flags ?? 0; }
  get tag() { return this.opts.tag ?? 'Untagged'; }
  get name() { return this.opts.name ?? ''; }
  hasCustomTag(t: string) { return this.opts.customTags?.includes(t) ?? false; }
}

export interface RaycastHit {
  point: Vector3;
  normal: Vector3;
  distance: number;
  collider: Collider;
}

// ---------------------------------------------------------------------------
// Building shapes

const INF = Number.POSITIVE_INFINITY;
const TERRAIN_AABB: AABB = { min: new Vector3(-INF, -INF, -INF), max: new Vector3(INF, 0, INF) };

function shapeAABB(s: Shape): AABB {
  switch (s.kind) {
    case 'box': {
      const b = s.obb.toBounds();
      return { min: b.min, max: b.max };
    }
    case 'sphere': {
      const r = new Vector3(s.r, s.r, s.r);
      return { min: s.c.sub(r), max: s.c.add(r) };
    }
    case 'capsule': {
      const r = new Vector3(s.r, s.r, s.r);
      return { min: Vector3.min(s.a, s.b).sub(r), max: Vector3.max(s.a, s.b).add(r) };
    }
    case 'mesh': {
      const t = s.tris;
      let x0 = INF, y0 = INF, z0 = INF, x1 = -INF, y1 = -INF, z1 = -INF;
      for (let i = 0; i < t.length; i += 3) {
        x0 = Math.min(x0, t[i]); y0 = Math.min(y0, t[i + 1]); z0 = Math.min(z0, t[i + 2]);
        x1 = Math.max(x1, t[i]); y1 = Math.max(y1, t[i + 1]); z1 = Math.max(z1, t[i + 2]);
      }
      return { min: new Vector3(x0, y0, z0), max: new Vector3(x1, y1, z1) };
    }
    case 'terrain':
      return { min: TERRAIN_AABB.min, max: new Vector3(INF, s.height, INF) };
  }
}

const aabbOverlap = (a: AABB, b: AABB) =>
  a.min.x <= b.max.x && a.max.x >= b.min.x && a.min.y <= b.max.y && a.max.y >= b.min.y && a.min.z <= b.max.z && a.max.z >= b.min.z;

const AXES = [Vector3.right, Vector3.up, Vector3.forward];

/** Instantiates a dumped collider under `pose`. Mesh data comes from `meshLookup`. */
export function colliderShape(
  c: ColliderJson,
  pose: Pose,
  meshLookup: (id: string) => { vertices: Float32Array; indices: Uint32Array } | null,
): Shape | null {
  const local = new Pose(Vector3.from(c.pos), Quaternion.from(c.rot));
  const scale = Vector3.from(c.scale);
  const rot = pose.rotation.mul(local.rotation);
  const toWorld = (p: Vector3) => pose.point(local.point(p.scale(scale)));
  const abs = (v: Vector3) => new Vector3(Math.abs(v.x), Math.abs(v.y), Math.abs(v.z));
  switch (c.type) {
    case 'Box': {
      const size = abs(Vector3.from(c.size!).scale(scale));
      return { kind: 'box', obb: OBB.fromSize(toWorld(Vector3.from(c.center!)), size, rot) };
    }
    case 'Sphere': {
      const s = abs(scale);
      return { kind: 'sphere', c: toWorld(Vector3.from(c.center!)), r: c.radius! * Math.max(s.x, s.y, s.z) };
    }
    case 'Capsule': {
      const dir = c.direction ?? 1;
      const s = abs(scale);
      const sa = [s.x, s.y, s.z];
      const height = c.height! * sa[dir];
      const r = c.radius! * Math.max(...sa.filter((_, i) => i !== dir));
      const half = Math.max(0, height / 2 - r);
      const center = toWorld(Vector3.from(c.center!));
      const axis = rot.rotate(AXES[dir]);
      return { kind: 'capsule', a: center.sub(axis.mul(half)), b: center.add(axis.mul(half)), r };
    }
    case 'Mesh': {
      const m = c.mesh ? meshLookup(c.mesh) : null;
      if (!m) return null;
      const tris = new Float64Array(m.indices.length * 3);
      const v = m.vertices;
      for (let i = 0; i < m.indices.length; i++) {
        const k = m.indices[i] * 3;
        const w = toWorld(new Vector3(v[k], v[k + 1], v[k + 2]));
        tris[i * 3] = w.x; tris[i * 3 + 1] = w.y; tris[i * 3 + 2] = w.z;
      }
      return { kind: 'mesh', tris, convex: !!c.convex };
    }
    default:
      return null;
  }
}

// ---------------------------------------------------------------------------
// Geometry helpers (plain numbers in hot paths)

type V = [number, number, number];
const sub = (a: V, b: V): V => [a[0] - b[0], a[1] - b[1], a[2] - b[2]];
const dot = (a: V, b: V) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
const cross = (a: V, b: V): V => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];
const add = (a: V, b: V): V => [a[0] + b[0], a[1] + b[1], a[2] + b[2]];
const mul = (a: V, s: number): V => [a[0] * s, a[1] * s, a[2] * s];
const len2 = (a: V) => dot(a, a);
const toV = (v: Vector3): V => [v.x, v.y, v.z];
const tri = (t: Float64Array, i: number): [V, V, V] => [
  [t[i], t[i + 1], t[i + 2]], [t[i + 3], t[i + 4], t[i + 5]], [t[i + 6], t[i + 7], t[i + 8]],
];

/** Closest point on triangle abc to p (Ericson, Real-Time Collision Detection 5.1.5). */
function closestPtTriangle(p: V, a: V, b: V, c: V): V {
  const ab = sub(b, a), ac = sub(c, a), ap = sub(p, a);
  const d1 = dot(ab, ap), d2 = dot(ac, ap);
  if (d1 <= 0 && d2 <= 0) return a;
  const bp = sub(p, b);
  const d3 = dot(ab, bp), d4 = dot(ac, bp);
  if (d3 >= 0 && d4 <= d3) return b;
  const vc = d1 * d4 - d3 * d2;
  if (vc <= 0 && d1 >= 0 && d3 <= 0) return add(a, mul(ab, d1 / (d1 - d3)));
  const cp = sub(p, c);
  const d5 = dot(ab, cp), d6 = dot(ac, cp);
  if (d6 >= 0 && d5 <= d6) return c;
  const vb = d5 * d2 - d1 * d6;
  if (vb <= 0 && d2 >= 0 && d6 <= 0) return add(a, mul(ac, d2 / (d2 - d6)));
  const va = d3 * d6 - d5 * d4;
  if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0) return add(b, mul(sub(c, b), (d4 - d3) / (d4 - d3 + (d5 - d6))));
  const denom = 1 / (va + vb + vc);
  return add(a, add(mul(ab, vb * denom), mul(ac, vc * denom)));
}

/** Squared distance between segments p1q1 and p2q2 (Ericson 5.1.9). */
function segSegDist2(p1: V, q1: V, p2: V, q2: V): number {
  const d1 = sub(q1, p1), d2 = sub(q2, p2), r = sub(p1, p2);
  const a = dot(d1, d1), e = dot(d2, d2), f = dot(d2, r);
  let s: number, t: number;
  const EPS = 1e-12;
  if (a <= EPS && e <= EPS) return len2(r);
  if (a <= EPS) {
    s = 0;
    t = Math.min(Math.max(f / e, 0), 1);
  } else {
    const c = dot(d1, r);
    if (e <= EPS) {
      t = 0;
      s = Math.min(Math.max(-c / a, 0), 1);
    } else {
      const b = dot(d1, d2);
      const denom = a * e - b * b;
      s = denom !== 0 ? Math.min(Math.max((b * f - c * e) / denom, 0), 1) : 0;
      t = (b * s + f) / e;
      if (t < 0) { t = 0; s = Math.min(Math.max(-c / a, 0), 1); }
      else if (t > 1) { t = 1; s = Math.min(Math.max((b - c) / a, 0), 1); }
    }
  }
  const c1 = add(p1, mul(d1, s)), c2 = add(p2, mul(d2, t));
  return len2(sub(c1, c2));
}

function pointSegDist2(p: V, a: V, b: V): number {
  const ab = sub(b, a);
  const l = len2(ab);
  const t = l > 0 ? Math.min(Math.max(dot(sub(p, a), ab) / l, 0), 1) : 0;
  return len2(sub(p, add(a, mul(ab, t))));
}

/** Does segment pq cross triangle abc (either side)? */
function segTriIntersect(p: V, q: V, a: V, b: V, c: V): boolean {
  const ab = sub(b, a), ac = sub(c, a), qp = sub(p, q);
  const n = cross(ab, ac);
  let d = dot(qp, n);
  let ap = sub(p, a);
  let t = dot(ap, n);
  if (d < 0) { d = -d; t = -t; ap = mul(ap, -1); }
  if (d === 0) return false;
  if (t < 0 || t > d) return false;
  const e = cross(qp, ap);
  let v = dot(ac, e);
  let w = -dot(ab, e);
  if (dot(qp, n) < 0) { v = -v; w = -w; }
  if (v < 0 || v > d) return false;
  if (w < 0 || v + w > d) return false;
  return true;
}

function segTriDist2(p: V, q: V, a: V, b: V, c: V): number {
  if (segTriIntersect(p, q, a, b, c)) return 0;
  return Math.min(
    len2(sub(p, closestPtTriangle(p, a, b, c))),
    len2(sub(q, closestPtTriangle(q, a, b, c))),
    segSegDist2(p, q, a, b),
    segSegDist2(p, q, b, c),
    segSegDist2(p, q, c, a),
  );
}

/** Box (center, axes, half extents) vs triangle, separating axis test (Akenine-Möller). */
function boxTri(center: V, axes: [V, V, V], ext: V, a: V, b: V, c: V): boolean {
  // Triangle into box space
  const toLocal = (p: V): V => {
    const d = sub(p, center);
    return [dot(d, axes[0]), dot(d, axes[1]), dot(d, axes[2])];
  };
  const v0 = toLocal(a), v1 = toLocal(b), v2 = toLocal(c);
  const f0 = sub(v1, v0), f1 = sub(v2, v1), f2 = sub(v0, v2);
  const E = 1e-9;
  const edgeAxes: V[] = [];
  for (const f of [f0, f1, f2]) {
    edgeAxes.push([0, -f[2], f[1]], [f[2], 0, -f[0]], [-f[1], f[0], 0]);
  }
  for (const ax of edgeAxes) {
    if (len2(ax) < 1e-18) continue;
    const p0 = dot(v0, ax), p1 = dot(v1, ax), p2 = dot(v2, ax);
    const r = ext[0] * Math.abs(ax[0]) + ext[1] * Math.abs(ax[1]) + ext[2] * Math.abs(ax[2]);
    if (Math.max(-Math.max(p0, p1, p2), Math.min(p0, p1, p2)) > r + E) return false;
  }
  for (let i = 0; i < 3; i++) {
    if (Math.max(v0[i], v1[i], v2[i]) < -ext[i] - E || Math.min(v0[i], v1[i], v2[i]) > ext[i] + E) return false;
  }
  const n = cross(f0, f1);
  const d = dot(n, v0);
  const r = ext[0] * Math.abs(n[0]) + ext[1] * Math.abs(n[1]) + ext[2] * Math.abs(n[2]);
  return Math.abs(d) <= r + E;
}

/** Squared distance from point to OBB. */
function pointObbDist2(p: V, obb: OBB): number {
  const cp = toV(obb.closestPoint(new Vector3(p[0], p[1], p[2])));
  return len2(sub(p, cp));
}

/** Squared distance from segment to OBB: convex in t, minimized by golden-section search. */
function segObbDist2(a: V, b: V, obb: OBB): number {
  const f = (t: number) => pointObbDist2(add(a, mul(sub(b, a), t)), obb);
  let lo = 0, hi = 1;
  const g = 0.3819660112501051;
  let x1 = lo + g * (hi - lo), x2 = hi - g * (hi - lo);
  let f1 = f(x1), f2 = f(x2);
  for (let i = 0; i < 80 && hi - lo > 1e-12; i++) {
    if (f1 < f2) { hi = x2; x2 = x1; f2 = f1; x1 = lo + g * (hi - lo); f1 = f(x1); }
    else { lo = x1; x1 = x2; f1 = f2; x2 = hi - g * (hi - lo); f2 = f(x2); }
  }
  return Math.min(f(0), f(1), f1, f2);
}

function obbParts(obb: OBB): { c: V; axes: [V, V, V]; e: V } {
  return { c: toV(obb.position), axes: [toV(obb.right), toV(obb.up), toV(obb.forward)], e: toV(obb.extents) };
}

/** For convex meshes: is point inside the closed hull (all faces see it from behind)? */
function insideConvex(p: V, tris: Float64Array): boolean {
  for (let i = 0; i < tris.length; i += 9) {
    const [a, b, c] = tri(tris, i);
    const n = cross(sub(b, a), sub(c, a));
    // Unity winding is clockwise seen from outside in a left-handed frame: cross(b-a, c-a) points outward.
    if (dot(n, sub(p, a)) > 1e-9) return false;
  }
  return true;
}

// ---------------------------------------------------------------------------
// Narrow phase

export function shapesOverlap(q: Shape, t: Shape): boolean {
  if (t.kind === 'terrain') {
    switch (q.kind) {
      case 'box': return q.obb.toBounds().min.y <= t.height;
      case 'sphere': return q.c.y - q.r <= t.height;
      case 'capsule': return Math.min(q.a.y, q.b.y) - q.r <= t.height;
      default: return false;
    }
  }
  if (q.kind === 'terrain') return shapesOverlap(t, q);
  if (q.kind === 'mesh') {
    if (t.kind === 'mesh') return false; // mesh-vs-mesh queries do not exist in Unity
    return shapesOverlap(t, q);
  }
  switch (q.kind) {
    case 'box': {
      const o = q.obb;
      switch (t.kind) {
        case 'box': return o.intersects(t.obb);
        case 'sphere': return pointObbDist2(toV(t.c), o) <= t.r * t.r;
        case 'capsule': return segObbDist2(toV(t.a), toV(t.b), o) <= t.r * t.r;
        case 'mesh': {
          const { c, axes, e } = obbParts(o);
          for (let i = 0; i < t.tris.length; i += 9) {
            const [a, b, cc] = tri(t.tris, i);
            if (boxTri(c, axes, e, a, b, cc)) return true;
          }
          return t.convex && insideConvex(c, t.tris);
        }
      }
      break;
    }
    case 'sphere': {
      const c = toV(q.c), r2 = q.r * q.r;
      switch (t.kind) {
        case 'box': return pointObbDist2(c, t.obb) <= r2;
        case 'sphere': return len2(sub(c, toV(t.c))) <= (q.r + t.r) ** 2;
        case 'capsule': return pointSegDist2(c, toV(t.a), toV(t.b)) <= (q.r + t.r) ** 2;
        case 'mesh': {
          for (let i = 0; i < t.tris.length; i += 9) {
            const [a, b, cc] = tri(t.tris, i);
            if (len2(sub(c, closestPtTriangle(c, a, b, cc))) <= r2) return true;
          }
          return t.convex && insideConvex(c, t.tris);
        }
      }
      break;
    }
    case 'capsule': {
      const a = toV(q.a), b = toV(q.b), r2 = q.r * q.r;
      switch (t.kind) {
        case 'box': return segObbDist2(a, b, t.obb) <= r2;
        case 'sphere': return pointSegDist2(toV(t.c), a, b) <= (q.r + t.r) ** 2;
        case 'capsule': return segSegDist2(a, b, toV(t.a), toV(t.b)) <= (q.r + t.r) ** 2;
        case 'mesh': {
          for (let i = 0; i < t.tris.length; i += 9) {
            const [x, y, z] = tri(t.tris, i);
            if (segTriDist2(a, b, x, y, z) <= r2) return true;
          }
          return t.convex && insideConvex(a, t.tris);
        }
      }
      break;
    }
  }
  return false;
}

// ---------------------------------------------------------------------------
// Raycasts

function rayShape(ray: Ray, s: Shape, maxDist: number): { t: number; n: Vector3 } | null {
  const o = toV(ray.origin), d = toV(ray.direction);
  switch (s.kind) {
    case 'terrain': {
      if (o[1] <= s.height || d[1] >= 0) return null;
      const t = (s.height - o[1]) / d[1];
      return t <= maxDist ? { t, n: Vector3.up } : null;
    }
    case 'box': {
      const obb = s.obb;
      if (obb.contains(ray.origin)) return null;
      const lo = obb.inverseTransform(ray.origin), ld = obb.inverseVector(ray.direction);
      const e = toV(obb.extents), lov = toV(lo), ldv = toV(ld);
      let tmin = -INF, tmax = INF, axis = -1, sign = 0;
      for (let i = 0; i < 3; i++) {
        if (Math.abs(ldv[i]) < 1e-15) {
          if (lov[i] < -e[i] || lov[i] > e[i]) return null;
          continue;
        }
        let t1 = (-e[i] - lov[i]) / ldv[i], t2 = (e[i] - lov[i]) / ldv[i];
        let s1 = -1;
        if (t1 > t2) { [t1, t2] = [t2, t1]; s1 = 1; }
        if (t1 > tmin) { tmin = t1; axis = i; sign = s1; }
        tmax = Math.min(tmax, t2);
        if (tmin > tmax) return null;
      }
      if (tmin < 0 || tmin > maxDist || axis < 0) return null;
      const localN: V = [0, 0, 0];
      localN[axis] = sign;
      return { t: tmin, n: obb.rotation.rotate(new Vector3(...localN)) };
    }
    case 'sphere': {
      const m = sub(o, toV(s.c));
      const c = dot(m, m) - s.r * s.r;
      if (c <= 0) return null;
      const b = dot(m, d);
      if (b > 0) return null;
      const disc = b * b - c;
      if (disc < 0) return null;
      const t = -b - Math.sqrt(disc);
      if (t > maxDist) return null;
      const p = add(o, mul(d, t));
      return { t, n: Vector3.from(sub(p, toV(s.c))).normalized };
    }
    case 'capsule': {
      if (pointSegDist2(o, toV(s.a), toV(s.b)) <= s.r * s.r) return null;
      // March-free exact test: min over the cylinder and the two end spheres.
      let best: { t: number; n: Vector3 } | null = null;
      for (const c of [s.a, s.b]) {
        const h = rayShape(ray, { kind: 'sphere', c, r: s.r }, maxDist);
        if (h && (!best || h.t < best.t)) best = h;
      }
      const a = toV(s.a), ab = sub(toV(s.b), a), ao = sub(o, a);
      const abab = dot(ab, ab), abd = dot(ab, d), abao = dot(ab, ao);
      const A = abab - abd * abd, B = abab * dot(ao, d) - abao * abd, C = abab * dot(ao, ao) - abao * abao - s.r * s.r * abab;
      if (A > 1e-12) {
        const disc = B * B - A * C;
        if (disc >= 0) {
          const t = (-B - Math.sqrt(disc)) / A;
          const y = abao + t * abd;
          if (t >= 0 && t <= maxDist && y >= 0 && y <= abab && (!best || t < best.t)) {
            const p = add(o, mul(d, t));
            const axisPt = add(a, mul(ab, y / abab));
            best = { t, n: Vector3.from(sub(p, axisPt)).normalized };
          }
        }
      }
      return best;
    }
    case 'mesh': {
      let best: { t: number; n: Vector3 } | null = null;
      for (let i = 0; i < s.tris.length; i += 9) {
        const [a, b, c] = tri(s.tris, i);
        const e1 = sub(b, a), e2 = sub(c, a);
        const n = cross(e1, e2);
        if (dot(n, d) >= 0) continue; // back face
        const pv = cross(d, e2);
        const det = dot(e1, pv);
        if (Math.abs(det) < 1e-15) continue;
        const inv = 1 / det;
        const tv = sub(o, a);
        const u = dot(tv, pv) * inv;
        if (u < 0 || u > 1) continue;
        const qv = cross(tv, e1);
        const v = dot(d, qv) * inv;
        if (v < 0 || u + v > 1) continue;
        const t = dot(e2, qv) * inv;
        if (t < 0 || t > maxDist) continue;
        if (!best || t < best.t) best = { t, n: Vector3.from(n).normalized };
      }
      return best;
    }
  }
}

// ---------------------------------------------------------------------------
// World

export class PhysicsWorld implements PhysicsQueries {
  readonly colliders = new Set<Collider>();
  private byEntity = new Map<SimEntity, Collider[]>();

  add(c: Collider) {
    this.colliders.add(c);
    if (c.entity) this.byEntity.set(c.entity, [...(this.byEntity.get(c.entity) ?? []), c]);
    return c;
  }

  remove(c: Collider) {
    this.colliders.delete(c);
    if (c.entity) this.byEntity.set(c.entity, (this.byEntity.get(c.entity) ?? []).filter((x) => x !== c));
  }

  removeEntity(e: SimEntity) {
    for (const c of this.byEntity.get(e) ?? []) this.colliders.delete(c);
    this.byEntity.delete(e);
  }

  entityColliders(e: SimEntity): Collider[] {
    return this.byEntity.get(e) ?? [];
  }

  private passes(c: Collider, mask: number, qti: QueryTriggerInteraction) {
    if (!c.enabled || ((mask >>> c.layer) & 1) === 0) return false;
    if (c.isTrigger) {
      if (qti === QueryTriggerInteraction.Ignore) return false;
      if (qti === QueryTriggerInteraction.UseGlobal && !queriesHitTriggers) return false;
    }
    return true;
  }

  overlap(shape: Shape, mask = -5, qti = QueryTriggerInteraction.UseGlobal): Collider[] {
    const box = shapeAABB(shape);
    const out: Collider[] = [];
    for (const c of this.colliders) {
      if (!this.passes(c, mask, qti) || !aabbOverlap(box, c.aabb)) continue;
      if (shapesOverlap(shape, c.shape)) out.push(c);
    }
    return out;
  }

  overlapOBB(obb: OBB, mask = -5, qti = QueryTriggerInteraction.Ignore) {
    return this.overlap({ kind: 'box', obb }, mask, qti);
  }
  overlapSphere(c: Vector3, r: number, mask = -5, qti = QueryTriggerInteraction.Ignore) {
    return this.overlap({ kind: 'sphere', c, r }, mask, qti);
  }
  overlapCapsule(a: Vector3, b: Vector3, r: number, mask = -5, qti = QueryTriggerInteraction.Ignore) {
    return this.overlap({ kind: 'capsule', a, b, r }, mask, qti);
  }
  checkOBB(obb: OBB, mask = -5, qti = QueryTriggerInteraction.UseGlobal) {
    return this.overlapOBB(obb, mask, qti).length > 0;
  }
  checkSphere(c: Vector3, r: number, mask = -5, qti = QueryTriggerInteraction.UseGlobal) {
    return this.overlapSphere(c, r, mask, qti).length > 0;
  }
  checkCapsule(a: Vector3, b: Vector3, r: number, mask = -5, qti = QueryTriggerInteraction.UseGlobal) {
    return this.overlapCapsule(a, b, r, mask, qti).length > 0;
  }

  raycastAll(ray: Ray, maxDistance = INF, mask = -5, qti = QueryTriggerInteraction.UseGlobal): RaycastHit[] {
    const hits: RaycastHit[] = [];
    for (const c of this.colliders) {
      if (!this.passes(c, mask, qti)) continue;
      const h = rayShape(ray, c.shape, maxDistance);
      if (h) hits.push({ point: ray.at(h.t), normal: h.n, distance: h.t, collider: c });
    }
    return hits.sort((a, b) => a.distance - b.distance);
  }

  raycast(ray: Ray, maxDistance = INF, mask = -5, qti = QueryTriggerInteraction.UseGlobal): RaycastHit | null {
    return this.raycastAll(ray, maxDistance, mask, qti)[0] ?? null;
  }

  raycastAny(origin: Vector3, direction: Vector3, maxDistance: number, layerMask: number): boolean {
    return this.raycast(new Ray(origin, direction), maxDistance, layerMask) != null;
  }

  /** Physics.Linecast */
  linecast(a: Vector3, b: Vector3, mask = -5, qti = QueryTriggerInteraction.UseGlobal): RaycastHit | null {
    const d = b.sub(a);
    return this.raycast(new Ray(a, d), d.magnitude, mask, qti);
  }
}
