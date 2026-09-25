// Game glue: player movement, aiming, building/hammer actions, HUD, save/load, undo.
import type { SimEntity } from '../core/entity';
import type { GameData, LoadedPrefab } from '../core/gamedata';
import { QueryTriggerInteraction } from '../core/physics';
import { BuildServer, Layer, type PlacementResult } from '../core/server';
import { AIM_ASSIST, aimTarget } from '../core/targeting';
import { Pose, Quaternion, Ray, Vector3 } from '../core/unity';
import { GROUP_ORDER, buildCatalog, type CatalogEntry } from './catalog';
import { Controls } from './controls';
import { RadialMenu, isBuildingPiece } from './radial';
import { Renderer, quatToThree, toThree } from './render';

const GRADE_NAMES = ['Twig', 'Wood', 'Stone', 'Metal', 'HQM'];
const SAVE_KEY = 'rustsim.save.v1';
const PREFS_KEY = 'rustsim.prefs.v1';
const SOLID_MASK = (1 << 0) | (1 << 8) | (1 << 16) | (1 << 21) | (1 << 23);

interface SaveFile {
  v: 1;
  entities: { path: string; pos: number[]; rot: number[]; grade: number; skin: number }[];
  player?: { pos: number[]; yaw: number; pitch: number };
}

const storage = {
  get(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  },
  set(key: string, v: string) {
    try {
      localStorage.setItem(key, v);
    } catch {
      /* private mode etc. */
    }
  },
};

const $ = <T extends HTMLElement = HTMLElement>(sel: string) => document.querySelector(sel) as T;

export class Game {
  srv: BuildServer;
  renderer: Renderer;
  controls: Controls;
  catalog: CatalogEntry[];

  feet = new Vector3(0, 0, -6);
  yaw = 0;
  pitch = 15;
  fly = true;
  crouch = false;
  vy = 0;

  mode: 'build' | 'hammer' = 'build';
  selected: CatalogEntry;
  grade = 0;
  skin = 0;
  rotation = Vector3.zero;
  showVolumes = false;
  private lastResult: PlacementResult | null = null;
  private lastTargetValid = false;
  private lookedAt: SimEntity | null = null;
  private undo: string[] = [];
  private toastTimer = 0;
  private lastTime = performance.now();
  private dirty = true;

  constructor(readonly data: GameData, canvas: HTMLCanvasElement) {
    this.srv = new BuildServer(data);
    this.renderer = new Renderer(canvas, this.srv);
    this.controls = new Controls(canvas, $('#pad'));
    this.catalog = buildCatalog(data);
    const radial = new RadialMenu(this.catalog, (e) => this.select(e));
    this.controls.radial = {
      open: (x, y) => radial.open(x, y),
      move: (x, y) => radial.move(x, y),
      end: (x, y) => radial.end(x, y),
    };
    this.selected = this.catalog.find((c) => c.prefab.path.endsWith('/foundation.prefab'))!;
    this.controls.onPrimary = () => this.primary();
    this.controls.onKey = (code) => this.key(code);
    this.loadPrefs();
    this.load();
    this.bindHud();
    this.updateHud();
    const onResize = () => this.renderer.resize(window.innerWidth, window.innerHeight);
    window.addEventListener('resize', onResize);
    onResize();
    requestAnimationFrame(() => this.frame());
  }

  // ---- persistence ---------------------------------------------------------

  serialize(): string {
    const f: SaveFile = {
      v: 1,
      entities: this.srv.entities.map((e) => ({
        path: e.prefab.path,
        pos: [e.pose.position.x, e.pose.position.y, e.pose.position.z],
        rot: [e.pose.rotation.x, e.pose.rotation.y, e.pose.rotation.z, e.pose.rotation.w],
        grade: e.grade,
        skin: e.skinID,
      })),
      player: { pos: [this.feet.x, this.feet.y, this.feet.z], yaw: this.yaw, pitch: this.pitch },
    };
    return JSON.stringify(f);
  }

  restore(json: string, withPlayer = true) {
    const f = JSON.parse(json) as SaveFile;
    for (const e of [...this.srv.entities]) this.srv.kill(e);
    this.srv.settle();
    for (const s of f.entities) {
      const p = this.data.byPath.get(s.path);
      if (!p) continue;
      this.srv.spawnPlaced(p, new Pose(Vector3.from(s.pos), Quaternion.from(s.rot)), undefined, { grade: s.grade, skin: s.skin });
    }
    this.srv.settle();
    if (withPlayer && f.player) {
      this.feet = Vector3.from(f.player.pos);
      this.yaw = f.player.yaw;
      this.pitch = f.player.pitch;
    }
    this.dirty = true;
  }

  save() {
    storage.set(SAVE_KEY, this.serialize());
  }

  load() {
    const s = storage.get(SAVE_KEY);
    if (s) {
      try {
        this.restore(s);
      } catch (e) {
        console.warn('save load failed', e);
      }
    }
  }

  private loadPrefs() {
    try {
      const p = JSON.parse(storage.get(PREFS_KEY) ?? '{}');
      if (typeof p.sens === 'number') this.controls.lookSensitivity = p.sens;
      if (typeof p.grade === 'number') this.grade = p.grade;
    } catch {
      /* ignore */
    }
  }

  private savePrefs() {
    storage.set(PREFS_KEY, JSON.stringify({ sens: this.controls.lookSensitivity, grade: this.grade }));
  }

  private checkpoint() {
    this.undo.push(this.serialize());
    if (this.undo.length > 100) this.undo.shift();
  }

  undoLast() {
    const s = this.undo.pop();
    if (!s) return this.toast('Nothing to undo');
    this.restore(s, false);
    this.save();
  }

  // ---- view / aim ----------------------------------------------------------

  /** PlayerEyes.EyeOffset (1.5) + DuckOffset (-0.6) when crouched. */
  get eyes() {
    return this.feet.add(new Vector3(0, this.crouch ? 0.9 : 1.5, 0));
  }

  get viewRotation() {
    return Quaternion.euler(this.pitch, this.yaw, 0);
  }

  get aimRay() {
    return new Ray(this.eyes, this.viewRotation.forward);
  }

  private move(dt: number) {
    const { dx, dy } = this.controls.consumeLook();
    const s = this.controls.state;
    this.yaw += dx * 0.12;
    this.pitch = Math.max(-89, Math.min(89, this.pitch + dy * 0.12));
    const yawQ = Quaternion.euler(0, this.yaw, 0);
    const fwd = yawQ.forward, right = yawQ.right;
    const speed = (this.fly ? 8 : this.crouch ? 1.7 : 5.5) * (s.sprint ? 2 : 1);
    let delta = fwd.mul(s.moveZ * speed * dt).add(right.mul(s.moveX * speed * dt));
    if (this.fly) {
      delta = delta.add(new Vector3(0, s.moveY * speed * dt, 0));
      this.feet = this.feet.add(delta);
      if (this.feet.y < 0) this.feet = this.feet.withY(0);
      return;
    }
    // Walking: gravity + simple capsule collision, one axis at a time.
    const blocked = (feet: Vector3) =>
      this.srv.physics.checkCapsule(feet.add(new Vector3(0, 0.55, 0)), feet.add(new Vector3(0, this.crouch ? 0.6 : 1.3, 0)), 0.45, SOLID_MASK & ~(1 << Layer.Terrain), QueryTriggerInteraction.Ignore);
    for (const d of [new Vector3(delta.x, 0, 0), new Vector3(0, 0, delta.z)]) {
      const next = this.feet.add(d);
      if (!blocked(next)) this.feet = next;
      else if (!blocked(next.add(new Vector3(0, 0.45, 0)))) this.feet = next.add(new Vector3(0, 0.45, 0)); // step up
    }
    this.vy -= 20 * dt;
    if (s.moveY > 0 && this.onGround()) this.vy = 6;
    const ny = this.feet.add(new Vector3(0, this.vy * dt, 0));
    const ground = this.srv.physics.raycast(new Ray(this.feet.add(new Vector3(0, 0.5, 0)), Vector3.down), 0.5 - this.vy * dt + 0.01, SOLID_MASK, QueryTriggerInteraction.Ignore);
    if (this.vy <= 0 && ground) {
      this.feet = this.feet.withY(ground.point.y);
      this.vy = 0;
    } else this.feet = ny;
    if (this.feet.y < 0) {
      this.feet = this.feet.withY(0);
      this.vy = 0;
    }
  }

  private onGround() {
    return !!this.srv.physics.raycast(new Ray(this.feet.add(new Vector3(0, 0.1, 0)), Vector3.down), 0.15, SOLID_MASK, QueryTriggerInteraction.Ignore);
  }

  private lookEntity(): SimEntity | null {
    const hit = this.srv.physics.raycast(this.aimRay, 6, (1 << Layer.Construction) | (1 << Layer.Deployed) | 1, QueryTriggerInteraction.Ignore);
    return hit?.collider.entity ?? null;
  }

  // ---- frame ---------------------------------------------------------------

  private frame() {
    const now = performance.now();
    const dt = Math.min(0.05, (now - this.lastTime) / 1000);
    this.lastTime = now;
    this.move(dt);
    this.srv.setPlayer({ position: this.feet, eyes: this.eyes, ducked: this.crouch });
    const cam = this.renderer.camera;
    cam.position.copy(toThree(this.eyes));
    cam.quaternion.copy(quatToThree(this.viewRotation));

    this.lookedAt = this.lookEntity();
    if (this.mode === 'build') {
      const prefab = this.selected.prefab;
      const t = aimTarget(this.srv, prefab, this.aimRay, { rotation: this.rotation, assist: AIM_ASSIST });
      this.lastTargetValid = t.valid;
      const r = t.valid ? this.srv.updatePlacement(prefab, t) : null;
      this.lastResult = r;
      this.renderer.setGhost(prefab, r ? new Pose(r.position, r.rotation) : null, !!r?.ok, this.showVolumes);
      this.renderer.setHighlight(null);
    } else {
      this.renderer.setGhost(null, null, false, false);
      this.renderer.setHighlight(this.lookedAt);
    }
    if (this.dirty) {
      this.renderer.sync();
      this.dirty = false;
    }
    this.updateInfo();
    this.renderer.render();
    requestAnimationFrame(() => this.frame());
  }

  // ---- actions -------------------------------------------------------------

  primary() {
    if (this.mode === 'build') this.place();
    else this.upgradeLooked();
  }

  place() {
    const prefab = this.selected.prefab;
    const t = aimTarget(this.srv, prefab, this.aimRay, { rotation: this.rotation, assist: AIM_ASSIST });
    if (!t.valid) return this.toast('Nothing to place on');
    this.checkpoint();
    const r = this.srv.build(prefab, t);
    if (!r.entity) {
      this.undo.pop();
      return this.toast(r.error || 'Cannot place here');
    }
    if (prefab.isBuildingBlock && (this.grade !== r.entity.grade || this.skin !== r.entity.skinID)) {
      const err = this.srv.upgrade(r.entity, this.grade, this.skin);
      if (err) this.toast(`Placed, upgrade failed: ${err}`);
    }
    this.srv.settle();
    this.dirty = true;
    this.save();
  }

  upgradeLooked() {
    const e = (this.lookedAt = this.lookEntity());
    if (!e || !this.srv.prefab(e).isBuildingBlock) return this.toast('Look at a building block');
    this.checkpoint();
    const err = this.srv.upgrade(e, this.grade, this.skin);
    if (err) {
      this.undo.pop();
      return this.toast(err);
    }
    this.srv.settle();
    this.dirty = true;
    this.save();
  }

  rotate() {
    if (this.mode === 'build') {
      const amt = this.selected.prefab.construction?.rotationAmount ?? new Vector3(0, 90, 0);
      this.rotation = new Vector3((this.rotation.x + amt.x) % 360, (this.rotation.y + amt.y) % 360, (this.rotation.z + amt.z) % 360);
      return;
    }
    const e = (this.lookedAt = this.lookEntity());
    if (!e) return this.toast('Look at something');
    this.checkpoint();
    const err = this.srv.rotate(e);
    if (err) {
      this.undo.pop();
      return this.toast(err);
    }
    this.srv.settle();
    this.dirty = true;
    this.save();
  }

  use() {
    const e = (this.lookedAt = this.lookEntity());
    if (!e || !this.srv.isDoor(e)) return this.toast('Nothing to use');
    this.checkpoint();
    this.srv.toggleDoor(e);
    this.dirty = true;
    this.save();
  }

  toggleCrouch() {
    this.crouch = !this.crouch;
    this.updateHud();
  }

  remove() {
    const e = (this.lookedAt = this.lookEntity());
    if (!e) return this.toast('Look at something');
    this.checkpoint();
    this.srv.demolish(e);
    this.srv.settle();
    this.dirty = true;
    this.save();
  }

  private key(code: string) {
    const map: Record<string, () => void> = {
      KeyR: () => this.rotate(),
      KeyE: () => this.use(),
      KeyH: () => this.setMode(this.mode === 'build' ? 'hammer' : 'build'),
      KeyC: () => this.toggleCrouch(),
      KeyX: () => this.remove(),
      KeyF: () => this.toggleFly(),
      KeyQ: () => this.openPicker(),
      KeyZ: () => this.undoLast(),
      KeyV: () => this.toggleVolumes(),
      KeyL: () => this.toggleLayers(),
      Digit1: () => this.setGrade(0, 0),
      Digit2: () => this.setGrade(1, 0),
      Digit3: () => this.setGrade(2, 0),
      Digit4: () => this.setGrade(3, 0),
      Digit5: () => this.setGrade(4, 0),
    };
    map[code]?.();
  }

  setMode(m: 'build' | 'hammer') {
    this.mode = m;
    this.updateHud();
  }

  setGrade(g: number, skin: number) {
    this.grade = g;
    this.skin = skin;
    this.savePrefs();
    this.updateHud();
  }

  toggleFly() {
    this.fly = !this.fly;
    this.vy = 0;
    this.updateHud();
  }

  toggleVolumes() {
    this.showVolumes = !this.showVolumes;
    this.updateHud();
  }

  toggleLayers() {
    this.renderer.mode = this.renderer.mode === 'normal' ? 'layers' : 'normal';
    this.dirty = true;
    this.updateHud();
  }

  select(entry: CatalogEntry) {
    this.selected = entry;
    this.rotation = Vector3.zero;
    this.setMode('build');
    this.closePicker();
  }

  // ---- HUD -----------------------------------------------------------------

  toast(msg: string) {
    const el = $('#toast');
    el.textContent = msg;
    el.classList.add('show');
    clearTimeout(this.toastTimer);
    this.toastTimer = window.setTimeout(() => el.classList.remove('show'), 1800);
  }

  private updateInfo() {
    const info = $('#info');
    const lines: string[] = [];
    if (this.mode === 'build') {
      const r = this.lastResult;
      if (!this.lastTargetValid) lines.push('<span class="bad">No target</span>');
      else if (r && !r.ok) lines.push(`<span class="bad">${escapeHtml(r.error)}</span>`);
    }
    const e = (this.lookedAt = this.lookEntity());
    if (e) {
      const p = this.srv.prefab(e);
      const name = this.srv.displayName(e);
      if (p.isBuildingBlock) {
        const skinName = this.srv.gradeDef(e)?.skin ? ` · skin ${this.srv.gradeDef(e)!.skin}` : '';
        lines.push(`${escapeHtml(name)} · ${GRADE_NAMES[e.grade]}${skinName} · <b>${Math.round(e.cachedStability * 100)}%</b>`);
      } else if (e.isStability) {
        lines.push(`${escapeHtml(name)} · <b>${Math.round(e.cachedStability * 100)}%</b>`);
      } else lines.push(escapeHtml(name));
    }
    const useBtn = $('#btn-use');
    const canUse = !!e && this.srv.isDoor(e);
    if ((useBtn.style.display !== 'none') !== canUse) useBtn.style.display = canUse ? '' : 'none';
    if (e && canUse) lines.push(`<span class="hint">${e.doorOpen ? 'USE: close' : 'USE: open'}</span>`);
    const html = lines.join('<br>');
    if (info.innerHTML !== html) info.innerHTML = html;
  }

  updateHud() {
    $('#item-name').textContent = this.selected.name;
    $('#btn-primary').textContent = this.mode === 'build' ? 'PLACE' : 'UPGRADE';
    $('#btn-mode').textContent = this.mode === 'build' ? '🔨' : '📐';
    $('#btn-mode').classList.toggle('on', this.mode === 'hammer');
    $('#btn-remove').style.display = this.mode === 'hammer' ? '' : 'none';
    $('#btn-fly').classList.toggle('on', this.fly);
    $('#btn-crouch').classList.toggle('on', this.crouch);
    $('#btn-vol').classList.toggle('on', this.showVolumes);
    $('#btn-layers').classList.toggle('on', this.renderer.mode === 'layers');
    $('#flybtns').style.display = this.fly ? '' : 'none';
    $('#btn-jump').style.display = this.fly ? 'none' : '';
    document.body.classList.toggle('flying', this.fly);
    document.querySelectorAll<HTMLElement>('.grade').forEach((el) => {
      el.classList.toggle('on', Number(el.dataset.grade) === this.grade);
    });
    const skins = this.skinsForGrade(this.grade);
    $('#grade-skin').textContent = this.skin ? skins.find((s) => s.skin === this.skin)?.name ?? `skin ${this.skin}` : '';
  }

  private skinsForGrade(grade: number) {
    // Grade skins as defined on the foundation (every building block shares the same skin set).
    const f = this.data.byPath.get('assets/prefabs/building core/foundation/foundation.prefab');
    return (f?.construction?.grades ?? [])
      .filter((g) => g.grade === grade)
      .map((g) => ({ skin: g.skin, name: skinLabel(this.data, g.skinPrefab) }));
  }

  private bindHud() {
    const tap = (sel: string, fn: () => void) => {
      const el = $(sel);
      el.addEventListener('touchstart', (e) => {
        e.preventDefault();
        e.stopPropagation();
        el.classList.add('pressed');
        fn();
      }, { passive: false });
      el.addEventListener('touchend', () => el.classList.remove('pressed'));
      el.addEventListener('mousedown', (e) => {
        e.stopPropagation();
        fn();
      });
    };
    const hold = (sel: string, axis: number) => {
      const el = $(sel);
      const set = (v: number) => (this.controls.state.moveY = v);
      el.addEventListener('touchstart', (e) => { e.preventDefault(); e.stopPropagation(); set(axis); }, { passive: false });
      el.addEventListener('touchend', () => set(0));
      el.addEventListener('touchcancel', () => set(0));
    };
    tap('#btn-primary', () => this.primary());
    tap('#btn-rotate', () => this.rotate());
    tap('#btn-mode', () => this.setMode(this.mode === 'build' ? 'hammer' : 'build'));
    tap('#btn-remove', () => this.remove());
    tap('#btn-use', () => this.use());
    tap('#btn-crouch', () => this.toggleCrouch());
    tap('#btn-undo', () => this.undoLast());
    tap('#btn-fly', () => this.toggleFly());
    tap('#btn-vol', () => this.toggleVolumes());
    tap('#btn-layers', () => this.toggleLayers());
    tap('#btn-menu', () => $('#menu').classList.toggle('show'));
    tap('#item', () => this.openPicker());
    hold('#btn-up', 1);
    hold('#btn-down', -1);
    hold('#btn-jump', 1);
    document.querySelectorAll<HTMLElement>('.grade').forEach((el) => {
      let timer = 0;
      const g = Number(el.dataset.grade);
      const start = (e: Event) => {
        e.preventDefault();
        e.stopPropagation();
        this.setGrade(g, 0);
        timer = window.setTimeout(() => this.openSkins(g), 450);
      };
      el.addEventListener('touchstart', start, { passive: false });
      el.addEventListener('mousedown', start);
      const end = () => clearTimeout(timer);
      el.addEventListener('touchend', end);
      el.addEventListener('mouseup', end);
      el.addEventListener('contextmenu', (e) => { e.preventDefault(); this.openSkins(g); });
    });

    // menu
    $('#m-clear').addEventListener('click', () => {
      if (!confirm('Remove everything?')) return;
      this.checkpoint();
      for (const e of [...this.srv.entities]) this.srv.kill(e);
      this.srv.settle();
      this.dirty = true;
      this.save();
    });
    $('#m-export').addEventListener('click', () => {
      const blob = new Blob([this.serialize()], { type: 'application/json' });
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `base-${Date.now()}.json`;
      a.click();
    });
    $<HTMLInputElement>('#m-import').addEventListener('change', async (ev) => {
      const f = (ev.target as HTMLInputElement).files?.[0];
      if (!f) return;
      this.checkpoint();
      this.restore(await f.text());
      this.save();
    });
    const sens = $<HTMLInputElement>('#m-sens');
    sens.value = String(this.controls.lookSensitivity);
    sens.addEventListener('input', () => {
      this.controls.lookSensitivity = Number(sens.value);
      this.savePrefs();
    });
    $('#m-full').addEventListener('click', () => document.documentElement.requestFullscreen?.().catch(() => {}));

    // picker
    const search = $<HTMLInputElement>('#picker-search');
    search.addEventListener('input', () => this.renderPicker(search.value));
    $('#picker-close').addEventListener('click', () => this.closePicker());
    $('#skins-close').addEventListener('click', () => $('#skins').classList.remove('show'));
  }

  openPicker() {
    $('#picker').classList.add('show');
    this.renderPicker($<HTMLInputElement>('#picker-search').value);
  }

  closePicker() {
    $('#picker').classList.remove('show');
  }

  private renderPicker(query: string) {
    const q = query.trim().toLowerCase();
    const list = $('#picker-list');
    list.innerHTML = '';
    // Building pieces live in the radial menu (long-press); the list is for items.
    const entries = this.catalog.filter((c) => !isBuildingPiece(c) && (!q || c.search.includes(q)));
    for (const g of GROUP_ORDER) {
      const items = entries.filter((e) => e.group === g);
      if (!items.length) continue;
      const h = document.createElement('div');
      h.className = 'picker-group';
      h.textContent = `${g} (${items.length})`;
      list.appendChild(h);
      for (const it of items) {
        const b = document.createElement('button');
        b.className = 'picker-item' + (it === this.selected ? ' on' : '');
        b.innerHTML = `${escapeHtml(it.name)}${it.dlc ? ' <span class="dlc">DLC</span>' : ''}`;
        b.addEventListener('click', () => this.select(it));
        list.appendChild(b);
      }
    }
  }

  private openSkins(grade: number) {
    const box = $('#skins-list');
    box.innerHTML = '';
    for (const s of this.skinsForGrade(grade)) {
      const b = document.createElement('button');
      b.className = 'picker-item' + (grade === this.grade && s.skin === this.skin ? ' on' : '');
      b.textContent = s.name;
      b.addEventListener('click', () => {
        this.setGrade(grade, s.skin);
        $('#skins').classList.remove('show');
      });
      box.appendChild(b);
    }
    $('#skins').classList.add('show');
  }
}

function skinLabel(data: GameData, skinPrefab: number | null): string {
  const p = skinPrefab != null ? data.prefabs.get(skinPrefab) : null;
  if (!p) return 'default';
  const parts = p.path.split('/').pop()!.replace('.prefab', '').split('.');
  return parts.slice(1).join('.') || 'default';
}

function escapeHtml(s: string) {
  return s.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]!);
}

export type { LoadedPrefab };
