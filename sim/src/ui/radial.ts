// Building-plan radial menu: long-press on the view, slide to a piece, release to pick.
// Inner ring = the 8 most used pieces, outer ring = the rest. Release in the centre cancels.
import type { CatalogEntry } from './catalog';

const INNER = ['foundation', 'foundation.triangle', 'wall', 'wall.doorway', 'wall.window', 'floor', 'floor.triangle', 'wall.frame'];
const OUTER = [
  'wall.half', 'wall.low', 'floor.frame', 'floor.triangle.frame', 'foundation.steps', 'block.stair.lshape',
  'block.stair.ushape', 'block.stair.spiral', 'block.stair.spiral.triangle', 'roof', 'roof.triangle', 'ramp',
];
const LABELS: Record<string, string> = {
  foundation: 'Foundation', 'foundation.triangle': 'Tri Found.', wall: 'Wall', 'wall.doorway': 'Doorway', 'wall.window': 'Window',
  floor: 'Floor', 'floor.triangle': 'Tri Floor', 'wall.frame': 'Wall Frame', 'wall.half': 'Half Wall', 'wall.low': 'Low Wall',
  'floor.frame': 'Floor Frame', 'floor.triangle.frame': 'Tri Frame', 'foundation.steps': 'Steps', 'block.stair.lshape': 'Stairs L',
  'block.stair.ushape': 'Stairs U', 'block.stair.spiral': 'Spiral', 'block.stair.spiral.triangle': 'Spiral Tri', roof: 'Roof',
  'roof.triangle': 'Tri Roof', ramp: 'Ramp',
};

const R_CENTER = 44;
// Rings are ellipses: phones are held in landscape, so there is more room sideways.
const R_INNER = { x: 190, y: 100 };
const R_OUTER = { x: 360, y: 170 };

export class RadialMenu {
  private el: HTMLElement;
  private items: { entry: CatalogEntry; ring: 0 | 1; angle: number; el: HTMLElement }[] = [];
  private cx = 0;
  private cy = 0;
  private hovered: CatalogEntry | null = null;

  constructor(catalog: CatalogEntry[], private onPick: (e: CatalogEntry) => void) {
    this.el = document.createElement('div');
    this.el.id = 'radial';
    document.body.appendChild(this.el);
    const find = (name: string) => catalog.find((c) => c.prefab.path.includes('/building core/') && c.prefab.path.endsWith(`/${name}.prefab`));
    const add = (names: string[], ring: 0 | 1) => {
      names.forEach((n, i) => {
        const entry = find(n);
        if (!entry) return;
        const angle = (i / names.length) * Math.PI * 2 - Math.PI / 2;
        const el = document.createElement('div');
        el.className = `radial-item ring${ring}`;
        el.textContent = LABELS[n] ?? entry.name;
        this.el.appendChild(el);
        this.items.push({ entry, ring, angle, el });
      });
    };
    add(INNER, 0);
    add(OUTER, 1);
  }

  open(_x: number, _y: number) {
    // Centred on screen so both rings always fit, whatever the thumb position.
    this.cx = window.innerWidth / 2;
    this.cy = window.innerHeight / 2;
    const scale = Math.min(1, (window.innerHeight / 2 - 22) / R_OUTER.y, (window.innerWidth / 2 - 60) / R_OUTER.x);
    for (const it of this.items) {
      const r = it.ring === 0 ? R_INNER : R_OUTER;
      it.el.style.left = `${this.cx + Math.cos(it.angle) * r.x * scale}px`;
      it.el.style.top = `${this.cy + Math.sin(it.angle) * r.y * scale}px`;
    }
    this.el.dataset.scale = String(scale);
    this.hovered = null;
    this.el.classList.add('show');
    document.body.classList.add('radial-open');
    this.highlight();
  }

  private pick(x: number, y: number): CatalogEntry | null {
    const scale = Number(this.el.dataset.scale ?? 1);
    // Work in "unit ellipse" space so angles and ring bands match what is drawn.
    const ex = (x - this.cx) / (R_INNER.x * scale), ey = (y - this.cy) / (R_INNER.y * scale);
    const d = Math.hypot(ex, ey);
    if (d < R_CENTER / R_INNER.y) return null;
    const mid = (1 + R_OUTER.y / R_INNER.y) / 2;
    const ring = d < mid ? 0 : 1;
    const a = Math.atan2(ey, ex);
    let best: (typeof this.items)[number] | null = null;
    let bestDiff = Infinity;
    for (const it of this.items) {
      if (it.ring !== ring) continue;
      let diff = Math.abs(a - it.angle) % (Math.PI * 2);
      if (diff > Math.PI) diff = Math.PI * 2 - diff;
      if (diff < bestDiff) { bestDiff = diff; best = it; }
    }
    return best?.entry ?? null;
  }

  move(x: number, y: number) {
    this.hovered = this.pick(x, y);
    this.highlight();
  }

  end(x: number, y: number) {
    const e = this.pick(x, y);
    this.el.classList.remove('show');
    document.body.classList.remove('radial-open');
    if (e) this.onPick(e);
  }

  private highlight() {
    for (const it of this.items) it.el.classList.toggle('on', it.entry === this.hovered);
  }
}

export const isBuildingPiece = (c: CatalogEntry) => c.prefab.path.includes('/building core/');
