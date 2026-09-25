// ModelConditionTest_* ports (Assembly-CSharp/ModelConditionTest_*.cs). They decide which
// ConditionalModel prefabs a building block skin spawns (wall ends, roof edges, foundation
// sides, ...). Those prefabs carry Construction-layer colliders, so they matter for placement.
import type { EntityLink, SimEntity } from './entity';
import { Vector3 } from './unity';

type J = any;

export interface ConditionHost {
  getEntityLinks(e: SimEntity): EntityLink[];
}

/** BuildingGrade.Enum */
export const Grade = { Twigs: 0, Wood: 1, Stone: 2, Metal: 3, TopTier: 4 } as const;

function findLink(host: ConditionHost, e: SimEntity, names: string | string[]): EntityLink | null {
  const list = typeof names === 'string' ? [names] : names;
  for (const l of host.getEntityLinks(e)) if (list.includes(l.socket.socketName)) return l;
  return null;
}

const occupied = (host: ConditionHost, e: SimEntity, name: string) => {
  const l = findLink(host, e, name);
  return l != null && !l.isEmpty();
};

const token = (e: SimEntity): string | null =>
  (e.prefab as { construction?: { raw?: { nameToken?: string } } }).construction?.raw?.nameToken ?? null;

const isBuildingBlock = (e: SimEntity) => (e.prefab as { isBuildingBlock?: boolean }).isBuildingBlock === true;

function signedAngle(from: Vector3, to: Vector3, axis: Vector3) {
  const a = Vector3.angle(from, to);
  const s = Vector3.dot(axis, Vector3.cross(from, to));
  return a * (s < 0 ? -1 : 1);
}

function wallTriangle(host: ConditionHost, e: SimEntity, right: boolean): boolean {
  for (const s of ['wall/sockets/wall-female', 'wall/sockets/floor-female/1', 'wall/sockets/floor-female/2', 'wall/sockets/floor-female/3', 'wall/sockets/floor-female/4']) {
    if (occupied(host, e, s)) return false;
  }
  if (occupied(host, e, right ? 'wall/sockets/stability/2' : 'wall/sockets/stability/1')) return false;
  const link = findLink(host, e, 'wall/sockets/neighbour/1');
  if (!link) return false;
  const fwd = e.pose.rotation.forward;
  for (const c of link.connections) {
    const bb = c.owner;
    if (!isBuildingBlock(bb)) continue;
    const other = right ? bb.pose.rotation.forward.neg() : bb.pose.rotation.forward;
    if (token(bb) === 'roof' && Vector3.angle(fwd, other) < 10) return true;
    if (token(bb) === 'roof_triangle' && Vector3.angle(fwd, other) < 40) return true;
  }
  return false;
}

function roofSide(host: ConditionHost, e: SimEntity, t: J, right: boolean): boolean {
  if (!isBuildingBlock(e)) return false;
  const link = findLink(host, e, right ? ['roof/sockets/neighbour/3', 'roof.triangle/sockets/neighbour/3'] : ['roof/sockets/neighbour/4', 'roof.triangle/sockets/neighbour/4']);
  if (!link) return false;
  const otherSuffix = right ? 'sockets/neighbour/4' : 'sockets/neighbour/3';
  const angle: number = t.angle ?? -1;
  const shape: number = t.shape ?? -1;
  if (angle === -1) {
    for (const c of link.connections) if (c.name.endsWith(otherSuffix)) return false;
    return true;
  }
  if (link.isEmpty()) return false;
  const convex = angle > 10;
  let result = false;
  for (const c of link.connections) {
    if (!c.name.endsWith(otherSuffix)) continue;
    if (shape === 0 && !c.name.startsWith('roof/')) continue;
    if (shape === 1 && !c.name.startsWith('roof.triangle/')) continue;
    const bb = c.owner;
    if (!isBuildingBlock(bb) || bb.grade !== e.grade) continue;
    let a = signedAngle(e.pose.rotation.forward, bb.pose.rotation.forward, Vector3.up);
    if (right) a = -a;
    if (a < angle - 10) {
      if (convex) return false;
    } else if (a > angle + 10) {
      if (convex) return false;
    } else result = true;
  }
  return result;
}

function roofEnd(host: ConditionHost, e: SimEntity, top: boolean): boolean {
  const rightNames = top ? ['roof/sockets/neighbour/5', 'roof.triangle/sockets/neighbour/5'] : ['roof/sockets/neighbour/3', 'roof.triangle/sockets/neighbour/3'];
  const leftNames = top ? ['roof/sockets/neighbour/6', 'roof.triangle/sockets/neighbour/6'] : ['roof/sockets/neighbour/4', 'roof.triangle/sockets/neighbour/4'];
  const wantRight = top ? 'sockets/neighbour/3' : 'sockets/neighbour/5';
  const wantLeft = top ? 'sockets/neighbour/4' : 'sockets/neighbour/6';
  const l1 = findLink(host, e, rightNames);
  if (!l1) return false;
  const f1 = l1.connections.some((c) => c.name.endsWith(wantRight));
  const l2 = findLink(host, e, leftNames);
  if (!l2) return false;
  const f2 = l2.connections.some((c) => c.name.endsWith(wantLeft));
  return !(f1 && f2);
}

/** Resolves ModelConditionTest_False/True.reference (a ConditionalModel by instanceID). */
export type ConditionalLookup = (instanceID: number | undefined) => { conditions: J[] } | null;

export function runTests(host: ConditionHost, e: SimEntity, conditions: J[], lookup: ConditionalLookup): boolean {
  for (const t of conditions) if (!doTest(host, e, t, lookup)) return false;
  return true;
}

export function doTest(host: ConditionHost, e: SimEntity, t: J, lookup: ConditionalLookup): boolean {
  switch (t.$type as string) {
    case 'ModelConditionTest_Wall':
      return !wallTriangle(host, e, false) && !wallTriangle(host, e, true);
    case 'ModelConditionTest_WallTriangleLeft':
      return wallTriangle(host, e, false);
    case 'ModelConditionTest_WallTriangleRight':
      return wallTriangle(host, e, true);
    case 'ModelConditionTest_RoofLeft':
      return roofSide(host, e, t, false);
    case 'ModelConditionTest_RoofRight':
      return roofSide(host, e, t, true);
    case 'ModelConditionTest_RoofBottom':
      return roofEnd(host, e, false);
    case 'ModelConditionTest_RoofTop':
      return roofEnd(host, e, true);
    case 'ModelConditionTest_RoofTriangle': {
      const l = findLink(host, e, 'roof/sockets/wall-female');
      return l == null || l.isEmpty();
    }
    case 'ModelConditionTest_RampLow': {
      const l = findLink(host, e, 'ramp/sockets/block-male/1');
      return l != null && !l.isEmpty();
    }
    case 'ModelConditionTest_RampHigh':
      return findLink(host, e, 'ramp/sockets/block-male/1')?.isEmpty() ?? false;
    case 'ModelConditionTest_SpiralStairs': {
      if (!isBuildingBlock(e)) return false;
      const l = findLink(host, e, ['block.stair.spiral/sockets/stairs-female/1', 'block.stair.spiral.triangle/sockets/stairs-female/1']);
      if (!l) return false;
      for (const c of l.connections) if (isBuildingBlock(c.owner) && c.owner.grade === e.grade) return false;
      const l2 = findLink(host, e, ['block.stair.spiral/sockets/floor-female/1', 'block.stair.spiral.triangle/sockets/floor-female/1']);
      return l2 == null || l2.isEmpty();
    }
    case 'ModelConditionTest_FoundationSide': {
      const socket = foundationSideSocket(t);
      const l = findLink(host, e, socket);
      if (!l) return false;
      for (const c of l.connections) {
        const bb = c.owner;
        if (!isBuildingBlock(bb) || token(bb) === 'foundation_steps') continue;
        if (bb.grade === Grade.TopTier || bb.grade === Grade.Metal || bb.grade === Grade.Stone) return false;
      }
      return true;
    }
    case 'ModelConditionTest_Variant':
      return variant(e.netId, BigInt(t.VariantSeed ?? 0), BigInt(t.VariantIndex ?? 0), BigInt(t.VariantCount ?? 3));
    case 'ModelConditionTest_False': {
      const ref = lookup(t.reference?.instanceID);
      // Unresolvable reference (dump without instanceIDs on refs): treat the referenced model as absent.
      return ref ? !runTests(host, e, ref.conditions, lookup) : true;
    }
    case 'ModelConditionTest_True': {
      const ref = lookup(t.reference?.instanceID);
      return ref ? runTests(host, e, ref.conditions, lookup) : false;
    }
    default:
      // Client-only tests (wallpaper, corners, inside/outside) never gate server colliders.
      return false;
  }
}

/** ModelConditionTest_FoundationSide.AttributeSetup */
function foundationSideSocket(t: J): string {
  const r: number[] = t.worldRotation ?? [0, 0, 0, 1];
  const [x, y, z, w] = r;
  // worldRotation * Vector3.right
  const vx = 1 - 2 * (y * y + z * z);
  const vz = 2 * (x * z - w * y);
  const tri = typeof t.hierachyName === 'string' && t.hierachyName.includes('foundation.triangle');
  let socket = '';
  if (tri) {
    if (vz < -0.9) socket = 'foundation.triangle/sockets/foundation-top/1';
    if (vx < -0.1) socket = 'foundation.triangle/sockets/foundation-top/2';
    if (vx > 0.1) socket = 'foundation.triangle/sockets/foundation-top/3';
    return socket;
  }
  if (vz < -0.9) socket = 'foundation/sockets/foundation-top/1';
  if (vz > 0.9) socket = 'foundation/sockets/foundation-top/3';
  if (vx < -0.9) socket = 'foundation/sockets/foundation-top/2';
  if (vx > 0.9) socket = 'foundation/sockets/foundation-top/4';
  return socket;
}

const M64 = (1n << 64n) - 1n;
/** SeedRandom.Wanghash(ref ulong) applied three times, as ModelConditionTest_Variant does. */
function variant(netId: number, seed: bigint, index: bigint, count: bigint): boolean {
  let x = (BigInt(netId) + seed) & M64;
  for (let i = 0; i < 3; i++) {
    x ^= x >> 30n;
    x = (x * 13787848793156543929n) & M64;
    x ^= x >> 27n;
    x = (x * 10723151780598845931n) & M64;
    x ^= x >> 31n;
  }
  return x % count === index;
}
