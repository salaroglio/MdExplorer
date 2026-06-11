import { Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { MarkAssistantService } from './mark-assistant.service';
import { MarkAction, MarkState, SpotlightRect } from './mark-types';

interface DialogPosition { left: number; top: number; }

const POSITION_KEY = 'mark.dialog.position';
/** Movement threshold (px) above which a mousedown→up is treated as a drag, not a click. */
const DRAG_CLICK_THRESHOLD = 5;

@Component({
  selector: 'app-mark-assistant',
  templateUrl: './mark-assistant.component.html',
  styleUrls: ['./mark-assistant.component.scss'],
})
export class MarkAssistantComponent implements OnInit, OnDestroy {
  @ViewChild('wrapRef', { static: false }) wrapRef!: ElementRef<HTMLElement>;

  state$!: Observable<MarkState>;
  text$!: Observable<string>;
  staticMode$!: Observable<boolean>;
  spotlight$!: Observable<SpotlightRect | null>;
  dim$!: Observable<boolean>;
  continueArrow$!: Observable<boolean>;
  isResponding$!: Observable<boolean>;
  isUndocked$!: Observable<boolean>;
  actions$!: Observable<MarkAction[] | null>;

  /** True only when running in Electron with the undock IPC bridge available. */
  canUndock = false;

  /** User input text bound via ngModel. */
  userInput = '';

  /** Custom position (top/left in px). null = use default bottom-right anchor. */
  position: DialogPosition | null = null;

  /** Drag state. */
  isDragging = false;
  private dragOffsetX = 0;
  private dragOffsetY = 0;
  /** Cumulative movement during current mousedown — drives click-vs-drag discrimination. */
  private dragMoveDistance = 0;

  constructor(public mark: MarkAssistantService) {}

  ngOnInit(): void {
    this.state$ = this.mark.state$;
    this.text$ = this.mark.text$;
    this.staticMode$ = this.mark.staticMode$;
    this.spotlight$ = this.mark.spotlight$;
    this.dim$ = this.mark.dim$;
    this.continueArrow$ = this.mark.continueArrow$;
    this.isResponding$ = this.mark.isResponding$;
    this.isUndocked$ = this.mark.isUndocked$;
    this.actions$ = this.mark.actions$;
    this.canUndock = this.mark.canUndock;

    this.loadPosition();
  }

  onUndockClick(event: Event): void {
    event.stopPropagation();
    this.mark.undock();
  }

  ngOnDestroy(): void {
    document.removeEventListener('mousemove', this.onDragMove);
    document.removeEventListener('mouseup', this.onDragEnd);
  }

  onDialogClick(): void {
    // Suppress the click if a drag just happened — otherwise dragging the
    // minimized badge would always re-summon Mark.
    if (this.dragMoveDistance > DRAG_CLICK_THRESHOLD) {
      this.dragMoveDistance = 0;
      return;
    }
    // Click on the minimized icon → open the idle menu (the action chooser),
    // NOT a replay of the welcome tour. The welcome tour is reserved to the
    // very-first launch; re-running it on click would feel "stuck on rewind".
    if (this.mark.currentState === 'minimized') {
      this.mark.openMenu();
    }
  }

  /** Tiny X button on the minimized icon → dismiss Mark entirely. */
  onCloseMiniClick(event: Event): void {
    event.stopPropagation();
    this.mark.hide();
  }

  onSkipClick(event: Event): void {
    event.stopPropagation();
    this.mark.skip();
  }

  /** User pressed Enter or clicked the send button. */
  async onSubmitInput(event?: Event): Promise<void> {
    if (event) event.stopPropagation();
    const text = this.userInput;
    if (!text || !text.trim()) return;
    this.userInput = '';
    await this.mark.submitUserInput(text);
  }

  /** User clicked one of the action buttons exposed by the current step. */
  async onActionClick(index: number, event: Event): Promise<void> {
    event.stopPropagation();
    await this.mark.submitAction(index);
  }

  // ── Drag and drop ────────────────────────────────────────────────────────

  /**
   * Mousedown anywhere on the wrap starts a drag, EXCEPT when the user
   * pressed on an interactive element (input, button) — those keep their
   * own behaviour.
   */
  onDragStart(event: MouseEvent): void {
    if (event.button !== 0) return; // only left button
    const t = event.target as HTMLElement;
    if (t.closest('input, textarea, button, .send-btn, .skip-btn')) return;

    const wrap = this.wrapRef?.nativeElement;
    if (!wrap) return;
    const rect = wrap.getBoundingClientRect();
    this.dragOffsetX = event.clientX - rect.left;
    this.dragOffsetY = event.clientY - rect.top;
    this.dragMoveDistance = 0;
    this.isDragging = true;

    // Switch from bottom/right anchoring to top/left at the current visible
    // position so the first mousemove doesn't snap the dialog elsewhere.
    this.position = this.clampToViewport(rect.left, rect.top);

    document.addEventListener('mousemove', this.onDragMove);
    document.addEventListener('mouseup', this.onDragEnd);
    event.preventDefault();
  }

  private onDragMove = (event: MouseEvent): void => {
    if (!this.isDragging) return;
    this.dragMoveDistance += Math.abs(event.movementX) + Math.abs(event.movementY);
    const x = event.clientX - this.dragOffsetX;
    const y = event.clientY - this.dragOffsetY;
    this.position = this.clampToViewport(x, y);
  };

  private onDragEnd = (): void => {
    if (!this.isDragging) return;
    this.isDragging = false;
    document.removeEventListener('mousemove', this.onDragMove);
    document.removeEventListener('mouseup', this.onDragEnd);
    this.savePosition();
  };

  @HostListener('window:resize')
  onWindowResize(): void {
    if (!this.position) return;
    this.position = this.clampToViewport(this.position.left, this.position.top);
    this.savePosition();
  }

  // ── Position persistence + viewport clamping ─────────────────────────────

  private clampToViewport(x: number, y: number): DialogPosition {
    const wrap = this.wrapRef?.nativeElement;
    const w = wrap?.offsetWidth ?? 540;
    const h = wrap?.offsetHeight ?? 200;
    const maxX = Math.max(0, window.innerWidth - w);
    const maxY = Math.max(0, window.innerHeight - h);
    return {
      left: Math.max(0, Math.min(x, maxX)),
      top: Math.max(0, Math.min(y, maxY)),
    };
  }

  private loadPosition(): void {
    try {
      const raw = localStorage.getItem(POSITION_KEY);
      if (!raw) return;
      const parsed = JSON.parse(raw) as DialogPosition;
      if (typeof parsed?.left === 'number' && typeof parsed?.top === 'number') {
        this.position = parsed;
        // Re-clamp on next tick so wrap dimensions are known
        setTimeout(() => {
          if (this.position) this.position = this.clampToViewport(this.position.left, this.position.top);
        }, 0);
      }
    } catch { /* ignore corrupted entries */ }
  }

  private savePosition(): void {
    if (this.position) {
      localStorage.setItem(POSITION_KEY, JSON.stringify(this.position));
    }
  }
}
