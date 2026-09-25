// Input: touch (arrow pad bottom-left, drag anywhere else to look, long-press to open the
// building radial) and keyboard + mouse for desktop.

export interface InputState {
  moveX: number; // strafe, -1..1
  moveZ: number; // forward, -1..1
  moveY: number; // fly up/down, -1..1
  lookDX: number; // accumulated pixels since last frame
  lookDY: number;
  sprint: boolean;
}

export interface RadialHandlers {
  open(x: number, y: number): void;
  move(x: number, y: number): void;
  end(x: number, y: number): void;
}

const LONG_PRESS_MS = 380;
const LONG_PRESS_SLOP = 12;

export class Controls {
  readonly state: InputState = { moveX: 0, moveZ: 0, moveY: 0, lookDX: 0, lookDY: 0, sprint: false };
  private padId: number | null = null;
  private lookId: number | null = null;
  private lookLast = { x: 0, y: 0 };
  private lookStart = { x: 0, y: 0 };
  private pressTimer = 0;
  private radialActive = false;
  private keys = new Set<string>();
  lookSensitivity = 0.25;
  onPrimary: () => void = () => {};
  onKey: (code: string) => void = () => {};
  radial: RadialHandlers | null = null;

  constructor(private surface: HTMLElement, private pad: HTMLElement) {
    pad.addEventListener('touchstart', (e) => this.padTouch(e), { passive: false });
    pad.addEventListener('touchmove', (e) => this.padTouch(e), { passive: false });
    pad.addEventListener('touchend', (e) => this.padEnd(e));
    pad.addEventListener('touchcancel', (e) => this.padEnd(e));

    surface.addEventListener('touchstart', (e) => this.lookStartEv(e), { passive: false });
    surface.addEventListener('touchmove', (e) => this.lookMove(e), { passive: false });
    surface.addEventListener('touchend', (e) => this.lookEnd(e));
    surface.addEventListener('touchcancel', (e) => this.lookEnd(e));

    window.addEventListener('keydown', (e) => {
      if ((e.target as HTMLElement)?.tagName === 'INPUT') return;
      this.keys.add(e.code);
      this.onKey(e.code);
      this.updateKeys();
    });
    window.addEventListener('keyup', (e) => {
      this.keys.delete(e.code);
      this.updateKeys();
    });
    surface.addEventListener('mousedown', (e) => {
      if (e.button === 0) {
        if (document.pointerLockElement !== surface) surface.requestPointerLock?.();
        else this.onPrimary();
      }
    });
    window.addEventListener('mousemove', (e) => {
      if (document.pointerLockElement === surface) {
        this.state.lookDX += e.movementX;
        this.state.lookDY += e.movementY;
      }
    });
  }

  private updateKeys() {
    const k = this.keys;
    this.state.moveZ = (k.has('KeyW') ? 1 : 0) - (k.has('KeyS') ? 1 : 0);
    this.state.moveX = (k.has('KeyD') ? 1 : 0) - (k.has('KeyA') ? 1 : 0);
    this.state.moveY = (k.has('Space') ? 1 : 0) - (k.has('ControlLeft') ? 1 : 0);
    this.state.sprint = k.has('ShiftLeft');
  }

  // ---- arrow pad: direction from the pad centre, 8-way, slide between arrows ----

  private padTouch(e: TouchEvent) {
    e.preventDefault();
    e.stopPropagation();
    const t = Array.from(e.changedTouches).find((x) => this.padId === null || x.identifier === this.padId);
    if (!t) return;
    this.padId = t.identifier;
    const r = this.pad.getBoundingClientRect();
    const dx = t.clientX - (r.left + r.width / 2);
    const dy = t.clientY - (r.top + r.height / 2);
    const dead = r.width * 0.12;
    if (Math.hypot(dx, dy) < dead) {
      this.setPad(0, 0);
      return;
    }
    const a = Math.round(Math.atan2(-dy, dx) / (Math.PI / 4)) * (Math.PI / 4);
    this.setPad(Math.round(Math.cos(a)), Math.round(Math.sin(a)));
  }

  private padEnd(e: TouchEvent) {
    if (Array.from(e.changedTouches).some((t) => t.identifier === this.padId)) {
      this.padId = null;
      this.setPad(0, 0);
    }
  }

  private setPad(x: number, z: number) {
    this.state.moveX = x;
    this.state.moveZ = z;
    this.pad.dataset.dir = `${x},${z}`;
  }

  // ---- look / long-press radial ----

  private lookStartEv(e: TouchEvent) {
    e.preventDefault();
    for (const t of Array.from(e.changedTouches)) {
      if (this.lookId !== null) continue;
      this.lookId = t.identifier;
      this.lookLast = { x: t.clientX, y: t.clientY };
      this.lookStart = { x: t.clientX, y: t.clientY };
      clearTimeout(this.pressTimer);
      this.pressTimer = window.setTimeout(() => {
        if (this.lookId === t.identifier && this.radial) {
          this.radialActive = true;
          this.radial.open(this.lookStart.x, this.lookStart.y);
        }
      }, LONG_PRESS_MS);
    }
  }

  private lookMove(e: TouchEvent) {
    e.preventDefault();
    for (const t of Array.from(e.changedTouches)) {
      if (t.identifier !== this.lookId) continue;
      if (this.radialActive) {
        this.radial?.move(t.clientX, t.clientY);
        continue;
      }
      if (Math.hypot(t.clientX - this.lookStart.x, t.clientY - this.lookStart.y) > LONG_PRESS_SLOP) clearTimeout(this.pressTimer);
      this.state.lookDX += (t.clientX - this.lookLast.x) / this.lookSensitivity / 2.5;
      this.state.lookDY += (t.clientY - this.lookLast.y) / this.lookSensitivity / 2.5;
      this.lookLast = { x: t.clientX, y: t.clientY };
    }
  }

  private lookEnd(e: TouchEvent) {
    for (const t of Array.from(e.changedTouches)) {
      if (t.identifier !== this.lookId) continue;
      clearTimeout(this.pressTimer);
      this.lookId = null;
      if (this.radialActive) {
        this.radialActive = false;
        this.radial?.end(t.clientX, t.clientY);
      }
    }
  }

  consumeLook() {
    const d = { dx: this.state.lookDX, dy: this.state.lookDY };
    this.state.lookDX = 0;
    this.state.lookDY = 0;
    return d;
  }
}
