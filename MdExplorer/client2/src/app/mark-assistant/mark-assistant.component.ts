import { Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { MarkAssistantService } from './mark-assistant.service';
import { MarkAction, MarkState, SpotlightRect } from './mark-types';

interface DialogPosition { left: number; top: number; }
/** Larghezza della finestra e altezza dell'area di testo, in px. */
interface DialogSize { width: number; textHeight: number; }

const POSITION_KEY = 'mark.dialog.position';
const SIZE_KEY = 'mark.dialog.size';

// Limiti del ridimensionamento. Il minimo tiene la faccia di Mark e una riga
// leggibile; il massimo evita che la finestra esca dallo schermo.
const MIN_WIDTH = 380;
const MAX_WIDTH = 1200;
const MIN_TEXT_HEIGHT = 80;
/** Movement threshold (px) above which a mousedown→up is treated as a drag, not a click. */
const DRAG_CLICK_THRESHOLD = 5;

@Component({
  selector: 'app-mark-assistant',
  templateUrl: './mark-assistant.component.html',
  styleUrls: ['./mark-assistant.component.scss'],
})
export class MarkAssistantComponent implements OnInit, OnDestroy {
  @ViewChild('wrapRef', { static: false }) wrapRef!: ElementRef<HTMLElement>;
  @ViewChild('textRef', { static: false }) textRef!: ElementRef<HTMLElement>;

  state$!: Observable<MarkState>;
  text$!: Observable<string>;
  staticMode$!: Observable<boolean>;
  spotlight$!: Observable<SpotlightRect | null>;
  dim$!: Observable<boolean>;
  continueArrow$!: Observable<boolean>;
  isResponding$!: Observable<boolean>;
  isUndocked$!: Observable<boolean>;
  actions$!: Observable<MarkAction[] | null>;
  /** Cronaca del "pensiero", mostrata nel fumetto fuori dalla box. */
  thinking$!: Observable<string[]>;

  /**
   * Il fumetto sta sopra la box, ma Mark si può trascinare ovunque: se è vicino
   * al bordo alto lo spazio sopra non c'è e il pensiero finirebbe fuori schermo.
   * In quel caso si ribalta sotto.
   */
  thinkBelow = false;
  private thinkSub: Subscription | null = null;

  /** True only when running in Electron with the undock IPC bridge available. */
  canUndock = false;

  /** User input text bound via ngModel. */
  userInput = '';

  /** Custom position (top/left in px). null = use default bottom-right anchor. */
  position: DialogPosition | null = null;

  /** Dimensione scelta dall'utente. null = quella di default definita nel CSS. */
  size: DialogSize | null = null;

  /** Testo di Mark reso in HTML: le risposte dell'AI arrivano in markdown. */
  renderedText$!: Observable<string>;

  /** Ridimensionamento in corso. */
  isResizing = false;
  private resizeStartX = 0;
  private resizeStartY = 0;
  private resizeStartWidth = 0;
  private resizeStartTextHeight = 0;
  private textSub: Subscription | null = null;

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
    this.thinking$ = this.mark.thinking$;
    this.canUndock = this.mark.canUndock;

    // Una sola conversione markdown, quella del servizio, condivisa con la finestra
    // staccata: due pipeline separate divergerebbero, e lo hanno gia' fatto.
    this.renderedText$ = this.mark.renderedText$;

    this.loadPosition();
    this.loadSize();

    // Mentre la risposta arriva in streaming, resta incollato in fondo — ma solo
    // se l'utente ci era già: se è risalito a rileggere, non glielo si strappa via.
    this.textSub = this.text$.subscribe(() => this.scheduleAutoScroll());
    this.thinkSub = this.thinking$.subscribe(() => this.scheduleThinkPlacement());
  }

  private scheduleAutoScroll(): void {
    setTimeout(() => {
      const el = this.textRef?.nativeElement;
      if (!el) return;
      const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
      if (distanceFromBottom < 80) el.scrollTop = el.scrollHeight;
    }, 0);
  }

  onUndockClick(event: Event): void {
    event.stopPropagation();
    this.mark.undock();
  }

  ngOnDestroy(): void {
    document.removeEventListener('mousemove', this.onDragMove);
    document.removeEventListener('mouseup', this.onDragEnd);
    document.removeEventListener('mousemove', this.onResizeMove);
    document.removeEventListener('mouseup', this.onResizeEnd);
    this.textSub?.unsubscribe();
    this.thinkSub?.unsubscribe();
  }

  /**
   * Decide se il fumetto sta sopra o sotto. La soglia è l'altezza massima che
   * il fumetto può occupare più il margine della coda.
   */
  private scheduleThinkPlacement(): void {
    setTimeout(() => {
      const wrap = this.wrapRef?.nativeElement;
      if (!wrap) return;
      this.thinkBelow = wrap.getBoundingClientRect().top < 260;
    }, 0);
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
    if (t.closest('input, textarea, button, .send-btn, .skip-btn, .resize-handle')) return;

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
    this.scheduleThinkPlacement();
  };

  @HostListener('window:resize')
  onWindowResize(): void {
    if (!this.position) return;
    this.position = this.clampToViewport(this.position.left, this.position.top);
    this.savePosition();
    this.scheduleThinkPlacement();
  }

  // ── Ridimensionamento ────────────────────────────────────────────────────

  /**
   * La maniglia sta nell'angolo OPPOSTO all'ancoraggio, così trascinarla muove
   * il bordo che si vede muovere:
   *
   *   - posizione di default (in basso a destra, fissa) → maniglia in alto a
   *     sinistra: allargare fa crescere la box verso sinistra e verso l'alto;
   *   - posizione scelta dall'utente (ancorata in alto a sinistra) → maniglia
   *     in basso a destra, dove la crescita avviene davvero.
   *
   * Il segno del delta si inverte di conseguenza: senza questo, in una delle
   * due modalità la box scapperebbe dalla direzione del mouse.
   */
  onResizeStart(event: MouseEvent): void {
    if (event.button !== 0) return;
    const wrap = this.wrapRef?.nativeElement;
    const text = this.textRef?.nativeElement;
    if (!wrap || !text) return;

    this.isResizing = true;
    this.resizeStartX = event.clientX;
    this.resizeStartY = event.clientY;
    this.resizeStartWidth = wrap.offsetWidth;
    this.resizeStartTextHeight = text.clientHeight;

    document.addEventListener('mousemove', this.onResizeMove);
    document.addEventListener('mouseup', this.onResizeEnd);
    event.preventDefault();
    event.stopPropagation();
  }

  private onResizeMove = (event: MouseEvent): void => {
    if (!this.isResizing) return;
    // -1 quando la maniglia è in alto a sinistra: allontanarsi (dx negativo)
    // deve ingrandire.
    const sign = this.position === null ? -1 : 1;
    const dx = (event.clientX - this.resizeStartX) * sign;
    const dy = (event.clientY - this.resizeStartY) * sign;

    const maxWidth = Math.min(MAX_WIDTH, window.innerWidth - 40);
    const maxTextHeight = Math.max(MIN_TEXT_HEIGHT, window.innerHeight * 0.8);

    this.size = {
      width: Math.round(Math.max(MIN_WIDTH, Math.min(this.resizeStartWidth + dx, maxWidth))),
      textHeight: Math.round(Math.max(MIN_TEXT_HEIGHT, Math.min(this.resizeStartTextHeight + dy, maxTextHeight))),
    };
  };

  private onResizeEnd = (): void => {
    if (!this.isResizing) return;
    this.isResizing = false;
    document.removeEventListener('mousemove', this.onResizeMove);
    document.removeEventListener('mouseup', this.onResizeEnd);
    this.saveSize();
    // Ingrandendo, la box potrebbe ora sporgere dallo schermo.
    if (this.position) {
      this.position = this.clampToViewport(this.position.left, this.position.top);
      this.savePosition();
    }
  };

  /** Torna alla dimensione di default (doppio click sulla maniglia). */
  onResizeReset(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.size = null;
    try { localStorage.removeItem(SIZE_KEY); } catch { /* storage non disponibile */ }
  }

  private loadSize(): void {
    try {
      const raw = localStorage.getItem(SIZE_KEY);
      if (!raw) return;
      const parsed = JSON.parse(raw) as DialogSize;
      if (typeof parsed?.width === 'number' && typeof parsed?.textHeight === 'number') {
        this.size = parsed;
      }
    } catch { /* voce corrotta: si riparte dal default */ }
  }

  private saveSize(): void {
    if (!this.size) return;
    try { localStorage.setItem(SIZE_KEY, JSON.stringify(this.size)); } catch { /* storage non disponibile */ }
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
