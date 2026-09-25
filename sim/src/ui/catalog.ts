// Everything placeable, grouped for the picker.
import type { GameData, LoadedPrefab } from '../core/gamedata';

export interface CatalogEntry {
  prefab: LoadedPrefab;
  name: string;
  group: string;
  dlc: boolean;
  search: string;
}

const BUILDING_ORDER = [
  'foundation', 'foundation.triangle', 'foundation.steps', 'wall', 'wall.half', 'wall.low', 'wall.doorway', 'wall.window',
  'wall.frame', 'floor', 'floor.triangle', 'floor.frame', 'floor.triangle.frame', 'roof', 'roof.triangle', 'ramp',
  'block.stair.lshape', 'block.stair.ushape', 'block.stair.spiral', 'block.stair.spiral.triangle',
];

const baseName = (p: LoadedPrefab) => p.path.split('/').pop()!.replace('.prefab', '');

function groupOf(p: LoadedPrefab): string {
  const path = p.path;
  if (path.includes('/building core/')) return 'Building';
  if (path.includes('/building boat/') || path.includes('/boat/')) return 'Boat';
  if (path.includes('/wallpaper/')) return 'Wallpaper';
  if (path.includes('/door') || path.includes('doors/')) return 'Doors & windows';
  if (path.includes('/playerioents/') || p.classes.includes('IOEntity')) return 'Electric & IO';
  if (path.includes('/xmas/') || path.includes('/halloween/') || path.includes('/easter/') || path.includes('/lunar') || path.includes('event')) return 'Events';
  if (path.includes('/misc/')) return 'Misc';
  return 'Deployables';
}

export function buildCatalog(data: GameData): CatalogEntry[] {
  const itemByPrefab = new Map<number, { name: string; dlc: boolean }>();
  for (const it of data.items) {
    if (it.deploys && !itemByPrefab.has(it.deploys)) itemByPrefab.set(it.deploys, { name: it.name, dlc: !!(it.dlc || it.steamItem) });
  }
  const out: CatalogEntry[] = [];
  for (const p of data.placeables) {
    const item = itemByPrefab.get(p.prefabID);
    const name = item?.name ?? p.construction?.name ?? baseName(p);
    const group = groupOf(p);
    out.push({ prefab: p, name: `${name}`, group, dlc: !!item?.dlc, search: `${name} ${baseName(p)} ${p.path}`.toLowerCase() });
  }
  const order = (e: CatalogEntry) => {
    const i = BUILDING_ORDER.indexOf(baseName(e.prefab));
    return i < 0 ? 1000 : i;
  };
  return out.sort((a, b) => (a.group === b.group ? order(a) - order(b) || a.name.localeCompare(b.name) : a.group.localeCompare(b.group)));
}

export const GROUP_ORDER = ['Building', 'Deployables', 'Doors & windows', 'Electric & IO', 'Wallpaper', 'Misc', 'Events', 'Boat'];
