// Input: touch (virtual stick on the left half, drag-to-look on the right half) and
// keyboard + mouse for desktop. Produces a per-frame movement vector and look deltas.

export interface InputState {
  moveX: number; // strafe, -1..1
  moveZ: number; // forward, -1..1
  moveY: number; // fly up/down, -1..1
  lookDX: number; // accumulated pixels since last frame
  lookDY: number;
  sprint: boolean;
}

export class Controls {
  readonly state: InputState = { moveX: 0, moveZ: 0, moveY: 0, lookDX: 0, lookDY: 0, sprint: false };
  private stickId: number | null = null;
  private stickOrigin = { x: 0, y: 0 };
  private lookId: number | null = null;
  private lookLast = { x: 0, y: 0 };
  private keys = new Set<string>();
  private stickEl: HTMLElement;
  private knobEl: HTMLElement;
  lookSensitivity = 0.25;
  /** Buttons pressed with the mouse on desktop map to these callbacks. */
  onPrimary: () => void = () => {};
  onKey: (code: string) => void = () => {};

  constructor(private surface: HTMLElement) {
    this.stickEl = document.createElement('div');
    this.stickEl.className = 'stick';
    this.knobEl = document.createElement('div');
    this.knobEl.className = 'stick-knob';
    this.stickEl.appendChild(this.knobEl);
    document.body.appendChild(this.stickEl);

    surface.addEventListener('touchstart', (e) => this.touchStart(e), { passive: false });
    surface.addEventListener('touchmove', (e) => this.touchMove(e), { passive: false });
    surface.addEventListener('touchend', (e) => this.touchEnd(e));
    surface.addEventListener('touchcancel', (e) => this.touchEnd(e));

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
    this.state.moveY = (k.has('Space') ? 1 : 0) - (k.has('ControlLeft') || k.has('KeyC') ? 1 : 0);
    this.state.sprint = k.has('ShiftLeft');
  }

  private touchStart(e: TouchEvent) {
    e.preventDefault();
    for (const t of Array.from(e.changedTouches)) {
      const leftHalf = t.clientX < window.innerWidth * 0.45;
      if (leftHalf && this.stickId === null) {
        this.stickId = t.identifier;
        this.stickOrigin = { x: t.clientX, y: t.clientY };
        this.stickEl.style.display = 'block';
        this.stickEl.style.left = `${t.clientX}px`;
        this.stickEl.style.top = `${t.clientY}px`;
        this.knobEl.style.transform = 'translate(-50%, -50%)';
      } else if (!leftHalf && this.lookId === null) {
        this.lookId = t.identifier;
        this.lookLast = { x: t.clientX, y: t.clientY };
      }
    }
  }

  private touchMove(e: TouchEvent) {
    e.preventDefault();
    for (const t of Array.from(e.changedTouches)) {
      if (t.identifier === this.stickId) {
        const R = 55;
        let dx = t.clientX - this.stickOrigin.x;
        let dy = t.clientY - this.stickOrigin.y;
        const m = Math.hypot(dx, dy);
        if (m > R) {
          dx *= R / m;
          dy *= R / m;
        }
        this.knobEl.style.transform = `translate(calc(-50% + ${dx}px), calc(-50% + ${dy}px))`;
        this.state.moveX = dx / R;
        this.state.moveZ = -dy / R;
        this.state.sprint = m > R * 1.6;
      } else if (t.identifier === this.lookId) {
        this.state.lookDX += (t.clientX - this.lookLast.x) / this.lookSensitivity / 2.5;
        this.state.lookDY += (t.clientY - this.lookLast.y) / this.lookSensitivity / 2.5;
        this.lookLast = { x: t.clientX, y: t.clientY };
      }
    }
  }

  private touchEnd(e: TouchEvent) {
    for (const t of Array.from(e.changedTouches)) {
      if (t.identifier === this.stickId) {
        this.stickId = null;
        this.stickEl.style.display = 'none';
        this.state.moveX = 0;
        this.state.moveZ = 0;
        this.state.sprint = false;
      } else if (t.identifier === this.lookId) {
        this.lookId = null;
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
