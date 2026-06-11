import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges,
  ViewChild
} from '@angular/core';

/**
 * Inline canvas editor for the project icon.
 *
 * UX:
 * - Big canvas with a dashed squircle in the middle (the crop frame).
 * - User pastes a logo with Ctrl+V (or "Incolla" button as fallback).
 * - Drag the image with the mouse to reposition; Ctrl + wheel to zoom.
 * - "Applica" produces a 256×256 PNG (squircle-clipped) and emits it as base64.
 * - "Rimuovi" emits null so the dialog can clear the custom icon.
 *
 * The component is dumb about persistence — the parent dialog decides when to
 * actually call the backend (so the icon change participates in the dialog's
 * Save / Cancel semantics together with name, description and participants).
 */
@Component({
  selector: 'app-icon-editor',
  templateUrl: './icon-editor.component.html',
  styleUrls: ['./icon-editor.component.scss']
})
export class IconEditorComponent implements AfterViewInit, OnChanges, OnDestroy {
  /** Current icon URL to preload into the canvas (for "edit existing icon"). */
  @Input() initialIconUrl: string | null = null;
  /** Whether a custom icon currently exists on the backend (drives "Rimuovi" visibility). */
  @Input() hasCustomIcon = false;

  /**
   * Emitted when the user clicks Applica/Rimuovi.
   * - { pngBase64: string }  → pending "set" with this PNG (data URL form)
   * - { pngBase64: null }    → pending "remove"
   */
  @Output() iconChanged = new EventEmitter<{ pngBase64: string | null }>();

  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

  // Canvas + crop geometry. Kept as constants so the math in applyAndEmit() stays simple.
  readonly canvasSize = 320;
  readonly cropSize = 200;          // dashed squircle side
  readonly outputSize = 256;        // exported PNG resolution
  readonly cornerRatio = 0.25;      // matches .project-icon-wrapper border-radius / size = 10/40

  // Image + transform state
  private img: HTMLImageElement | null = null;
  private offsetX = 0;
  private offsetY = 0;
  private scale = 1;
  private minScale = 0.05;
  private maxScale = 8;

  // Drag state
  private dragging = false;
  private dragStartX = 0;
  private dragStartY = 0;
  private dragStartOffsetX = 0;
  private dragStartOffsetY = 0;

  // UI flags
  hasImage = false;
  pasteError: string | null = null;
  pasting = false;

  ngAfterViewInit(): void {
    this.draw();
    if (this.initialIconUrl) {
      this.loadFromUrl(this.initialIconUrl);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.initialIconUrl && !changes.initialIconUrl.isFirstChange()) {
      const url = changes.initialIconUrl.currentValue as string | null;
      if (url) {
        this.loadFromUrl(url);
      } else {
        this.clear();
      }
    }
  }

  ngOnDestroy(): void {
    this.img = null;
  }

  // ─── Paste handling ────────────────────────────────────────────────

  /**
   * Document-level paste listener: when the dialog is open and the user hits
   * Ctrl+V, we accept the first image item. Scoped to "this component is
   * mounted" — the dialog is the only consumer so cross-talk is unlikely.
   */
  @HostListener('document:paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    const items = event.clipboardData?.items;
    if (!items) return;

    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      if (item.kind === 'file' && item.type.startsWith('image/')) {
        const blob = item.getAsFile();
        if (blob) {
          event.preventDefault();
          this.loadFromBlob(blob);
          return;
        }
      }
    }
  }

  /**
   * Programmatic clipboard read for the "Incolla" fallback button.
   * Uses the async Clipboard API; in Electron this usually works without
   * explicit permission because the page is treated as same-origin.
   */
  async pasteFromClipboard(): Promise<void> {
    this.pasteError = null;
    this.pasting = true;
    try {
      const items = await (navigator.clipboard as any).read?.();
      if (!items) {
        this.pasteError = 'CLIPBOARD_UNAVAILABLE';
        return;
      }
      for (const item of items) {
        const imgType = item.types.find((t: string) => t.startsWith('image/'));
        if (imgType) {
          const blob = await item.getType(imgType);
          this.loadFromBlob(blob);
          return;
        }
      }
      this.pasteError = 'NO_IMAGE';
    } catch {
      this.pasteError = 'CLIPBOARD_DENIED';
    } finally {
      this.pasting = false;
    }
  }

  private loadFromBlob(blob: Blob): void {
    const url = URL.createObjectURL(blob);
    this.loadFromUrl(url, () => URL.revokeObjectURL(url));
  }

  private loadFromUrl(url: string, onLoad?: () => void): void {
    const image = new Image();
    image.crossOrigin = 'anonymous';
    image.onload = () => {
      this.img = image;
      this.hasImage = true;
      this.pasteError = null;
      this.fitImageToCrop();
      this.draw();
      onLoad?.();
    };
    image.onerror = () => {
      this.pasteError = 'LOAD_ERROR';
      onLoad?.();
    };
    image.src = url;
  }

  /** Initial scale so the longest side fills the crop frame. */
  private fitImageToCrop(): void {
    if (!this.img) return;
    const longest = Math.max(this.img.width, this.img.height);
    this.scale = longest > 0 ? this.cropSize / longest : 1;
    this.offsetX = 0;
    this.offsetY = 0;
  }

  // ─── Mouse interactions ────────────────────────────────────────────

  onMouseDown(event: MouseEvent): void {
    if (!this.img) return;
    this.dragging = true;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.dragStartOffsetX = this.offsetX;
    this.dragStartOffsetY = this.offsetY;
    event.preventDefault();
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    if (!this.dragging) return;
    this.offsetX = this.dragStartOffsetX + (event.clientX - this.dragStartX);
    this.offsetY = this.dragStartOffsetY + (event.clientY - this.dragStartY);
    this.draw();
  }

  @HostListener('document:mouseup')
  onMouseUp(): void {
    this.dragging = false;
  }

  /** Ctrl + wheel to zoom; zooms toward the canvas center to keep things predictable. */
  onWheel(event: WheelEvent): void {
    // Block the Electron app-wide zoom regardless of whether we have an image:
    // ElectronMdExplorer/preload.js listens on `window` for ctrl+wheel and
    // forwards a `zoom-mouse-wheel` IPC that calls webContents.setZoomFactor.
    // Without stopPropagation the same gesture would zoom both the local crop
    // AND the entire app shell. preventDefault alone isn't enough — it stops
    // the browser default action but not other listeners on ancestor nodes.
    if (event.ctrlKey) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (!this.img) return;
    if (!event.ctrlKey) return;          // plain wheel keeps page scroll behavior
    const factor = event.deltaY < 0 ? 1.1 : 1 / 1.1;
    const next = Math.min(this.maxScale, Math.max(this.minScale, this.scale * factor));
    this.scale = next;
    this.draw();
  }

  // ─── Rendering ─────────────────────────────────────────────────────

  private draw(): void {
    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Background (checkerboard would be overkill here; a soft neutral works).
    ctx.fillStyle = '#0f1419';
    ctx.fillRect(0, 0, this.canvasSize, this.canvasSize);

    if (this.img) {
      const w = this.img.width * this.scale;
      const h = this.img.height * this.scale;
      const x = this.canvasSize / 2 + this.offsetX - w / 2;
      const y = this.canvasSize / 2 + this.offsetY - h / 2;
      ctx.drawImage(this.img, x, y, w, h);
    }

    // Dim everything outside the crop frame so the user sees what will be cut.
    const cropX = (this.canvasSize - this.cropSize) / 2;
    const cropY = (this.canvasSize - this.cropSize) / 2;
    const radius = this.cropSize * this.cornerRatio;

    ctx.save();
    ctx.fillStyle = 'rgba(0, 0, 0, 0.55)';
    ctx.beginPath();
    ctx.rect(0, 0, this.canvasSize, this.canvasSize);
    this.roundedRectPath(ctx, cropX, cropY, this.cropSize, this.cropSize, radius, true);
    ctx.fill('evenodd');
    ctx.restore();

    // Dashed squircle outline for the crop frame.
    ctx.save();
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;
    ctx.setLineDash([8, 6]);
    this.roundedRectPath(ctx, cropX, cropY, this.cropSize, this.cropSize, radius);
    ctx.stroke();
    ctx.restore();
  }

  /**
   * Standard rounded-rect path. Optional `counterclockwise` flag is used when
   * we need the inner cutout for the dim overlay (even-odd fill rule).
   */
  private roundedRectPath(
    ctx: CanvasRenderingContext2D,
    x: number, y: number, w: number, h: number, r: number,
    counterclockwise = false
  ): void {
    ctx.moveTo(x + r, y);
    if (counterclockwise) {
      ctx.lineTo(x + r, y);
      ctx.arcTo(x, y, x, y + r, r);
      ctx.lineTo(x, y + h - r);
      ctx.arcTo(x, y + h, x + r, y + h, r);
      ctx.lineTo(x + w - r, y + h);
      ctx.arcTo(x + w, y + h, x + w, y + h - r, r);
      ctx.lineTo(x + w, y + r);
      ctx.arcTo(x + w, y, x + w - r, y, r);
      ctx.closePath();
    } else {
      ctx.lineTo(x + w - r, y);
      ctx.arcTo(x + w, y, x + w, y + r, r);
      ctx.lineTo(x + w, y + h - r);
      ctx.arcTo(x + w, y + h, x + w - r, y + h, r);
      ctx.lineTo(x + r, y + h);
      ctx.arcTo(x, y + h, x, y + h - r, r);
      ctx.lineTo(x, y + r);
      ctx.arcTo(x, y, x + r, y, r);
      ctx.closePath();
    }
  }

  // ─── Output ────────────────────────────────────────────────────────

  applyAndEmit(): void {
    if (!this.img) return;

    const out = document.createElement('canvas');
    out.width = this.outputSize;
    out.height = this.outputSize;
    const ctx = out.getContext('2d');
    if (!ctx) return;

    // Map from "canvas crop frame coords" to "output coords":
    //   k = outputSize / cropSize
    // Image position relative to crop frame top-left:
    //   relX = cropSize/2 + offsetX - img.width  * scale / 2
    //   relY = cropSize/2 + offsetY - img.height * scale / 2
    const k = this.outputSize / this.cropSize;
    const relX = (this.cropSize / 2) + this.offsetX - (this.img.width * this.scale / 2);
    const relY = (this.cropSize / 2) + this.offsetY - (this.img.height * this.scale / 2);

    // Squircle clip — matches the card icon shape so the saved PNG looks the
    // same in both the editor preview and the project list card.
    ctx.save();
    ctx.beginPath();
    this.roundedRectPath(ctx, 0, 0, this.outputSize, this.outputSize, this.outputSize * this.cornerRatio);
    ctx.clip();
    ctx.drawImage(
      this.img,
      relX * k,
      relY * k,
      this.img.width * this.scale * k,
      this.img.height * this.scale * k
    );
    ctx.restore();

    const dataUrl = out.toDataURL('image/png');
    this.iconChanged.emit({ pngBase64: dataUrl });
  }

  removeAndEmit(): void {
    this.clear();
    this.iconChanged.emit({ pngBase64: null });
  }

  resetView(): void {
    if (!this.img) return;
    this.fitImageToCrop();
    this.draw();
  }

  private clear(): void {
    this.img = null;
    this.hasImage = false;
    this.offsetX = 0;
    this.offsetY = 0;
    this.scale = 1;
    this.draw();
  }
}
