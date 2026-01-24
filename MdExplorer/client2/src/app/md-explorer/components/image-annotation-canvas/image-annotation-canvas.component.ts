import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';

export type MarkerType = 'ball' | 'box';

export interface AnnotationMarker {
  id: number;
  type: MarkerType;
  x: number;      // percentage relative to image width (for ball: center, for box: top-left corner)
  y: number;      // percentage relative to image height
  color: string;  // individual marker color
  // Only for box:
  width?: number;   // percentage width
  height?: number;  // percentage height
}

export interface MarkerColor {
  name: string;
  value: string;
}

export const MARKER_COLORS: MarkerColor[] = [
  { name: 'Rosso', value: '#FF4444' },
  { name: 'Blu', value: '#2196F3' },
  { name: 'Verde', value: '#4CAF50' },
  { name: 'Arancione', value: '#FF9800' },
  { name: 'Viola', value: '#9C27B0' },
  { name: 'Teal', value: '#009688' }
];

export type ResizeHandle = 'nw' | 'ne' | 'sw' | 'se' | 'n' | 's' | 'e' | 'w';

@Component({
  selector: 'app-image-annotation-canvas',
  templateUrl: './image-annotation-canvas.component.html',
  styleUrls: ['./image-annotation-canvas.component.scss']
})
export class ImageAnnotationCanvasComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('canvasContainer', { static: true }) containerRef!: ElementRef<HTMLDivElement>;

  @Input() imageBlob!: Blob;
  @Input() markerColor: string = MARKER_COLORS[0].value;
  @Output() markersChanged = new EventEmitter<AnnotationMarker[]>();

  markers: AnnotationMarker[] = [];
  availableColors = MARKER_COLORS;

  // Current tool
  currentTool: MarkerType = 'ball';

  // Cursor state
  currentCursor: string = 'crosshair';

  private canvas!: HTMLCanvasElement;
  private ctx!: CanvasRenderingContext2D;
  private image: HTMLImageElement | null = null;
  private imageLoaded = false;
  private resizeObserver?: any; // ResizeObserver

  // Configurable marker radius
  markerRadius = 16;
  readonly MIN_MARKER_RADIUS = 8;
  readonly MAX_MARKER_RADIUS = 32;

  // Configurable image padding (margin around image for annotations at edges)
  imagePadding = 0; // pixels in canvas coordinates
  readonly MAX_IMAGE_PADDING = 50;

  // Cached image display dimensions (used for drawing and coordinate calculations)
  private imageDisplayWidth = 0;
  private imageDisplayHeight = 0;

  // Drag state
  private isDragging = false;
  private isResizing = false;
  private draggedMarker: AnnotationMarker | null = null;
  private resizeHandle: ResizeHandle | null = null;
  private dragOffset = { x: 0, y: 0 };

  // Box drawing state
  private isDrawingBox = false;
  private boxStartPoint: { x: number, y: number } | null = null;
  private boxPreview: { x: number, y: number, width: number, height: number } | null = null;

  // Resize handle size in pixels
  private readonly HANDLE_SIZE = 8;
  // Percentage radius for hit testing balls
  private readonly BALL_HIT_RADIUS_PERCENT = 3;
  // Flag to hide handles during export
  private isExportMode = false;

  ngOnInit(): void {
    console.log('[ImageAnnotationCanvas] Initializing component');
  }

  ngAfterViewInit(): void {
    this.canvas = this.canvasRef.nativeElement;
    this.ctx = this.canvas.getContext('2d')!;

    if (this.imageBlob) {
      this.loadImage();
    }

    // Setup resize observer to handle container resize
    if (typeof (window as any).ResizeObserver !== 'undefined') {
      this.resizeObserver = new (window as any).ResizeObserver(() => {
        if (this.imageLoaded) {
          this.resizeCanvas();
          this.redraw();
        }
      });
      this.resizeObserver.observe(this.containerRef.nativeElement);
    }
  }

  ngOnDestroy(): void {
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
  }

  private loadImage(): void {
    const url = URL.createObjectURL(this.imageBlob);
    this.image = new Image();
    this.image.onload = () => {
      console.log('[ImageAnnotationCanvas] Image loaded:', this.image!.width, 'x', this.image!.height);
      this.imageLoaded = true;
      this.resizeCanvas();
      this.redraw();
      URL.revokeObjectURL(url);
    };
    this.image.onerror = (err) => {
      console.error('[ImageAnnotationCanvas] Failed to load image:', err);
      URL.revokeObjectURL(url);
    };
    this.image.src = url;
  }

  private resizeCanvas(): void {
    if (!this.image || !this.containerRef) return;

    // Always use original image dimensions (1:1, no scaling)
    // Container has overflow:auto so user can scroll if image is large
    this.imageDisplayWidth = this.image.width;
    this.imageDisplayHeight = this.image.height;

    // Canvas size = image size + padding on all sides
    // Padding EXPANDS the canvas, doesn't shrink the image
    this.canvas.width = this.imageDisplayWidth + (this.imagePadding * 2);
    this.canvas.height = this.imageDisplayHeight + (this.imagePadding * 2);

    console.log('[ImageAnnotationCanvas] Canvas resized to:', this.canvas.width, 'x', this.canvas.height,
                'image:', this.imageDisplayWidth, 'x', this.imageDisplayHeight, 'padding:', this.imagePadding);
  }

  private redraw(): void {
    if (!this.ctx || !this.image || !this.imageLoaded) return;

    // Clear canvas
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    // Fill padding area with background color
    if (this.imagePadding > 0) {
      this.ctx.fillStyle = '#e8e8e8';
      this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
    }

    // Draw image at stored dimensions (inside the padding area)
    this.ctx.drawImage(this.image, this.imagePadding, this.imagePadding,
                       this.imageDisplayWidth, this.imageDisplayHeight);

    // Draw markers
    this.markers.forEach(marker => {
      this.drawMarker(marker);
    });
  }

  private drawMarker(marker: AnnotationMarker): void {
    if (marker.type === 'ball') {
      this.drawBallMarker(marker);
    } else {
      this.drawBoxMarker(marker);
    }
  }

  private drawBallMarker(marker: AnnotationMarker): void {
    // Marker coordinates are relative to image area, add padding offset
    const x = this.imagePadding + (marker.x / 100) * this.imageDisplayWidth;
    const y = this.imagePadding + (marker.y / 100) * this.imageDisplayHeight;

    // Draw circle with shadow
    this.ctx.save();
    this.ctx.shadowColor = 'rgba(0, 0, 0, 0.4)';
    this.ctx.shadowBlur = 4;
    this.ctx.shadowOffsetX = 2;
    this.ctx.shadowOffsetY = 2;

    this.ctx.beginPath();
    this.ctx.arc(x, y, this.markerRadius, 0, Math.PI * 2);
    this.ctx.fillStyle = marker.color; // Use individual marker color
    this.ctx.fill();

    // Draw border
    this.ctx.shadowColor = 'transparent';
    this.ctx.strokeStyle = '#FFFFFF';
    this.ctx.lineWidth = 2;
    this.ctx.stroke();

    // Draw number
    this.ctx.fillStyle = '#FFFFFF';
    this.ctx.font = 'bold 14px Arial';
    this.ctx.textAlign = 'center';
    this.ctx.textBaseline = 'middle';
    this.ctx.fillText(marker.id.toString(), x, y);

    this.ctx.restore();
  }

  private drawBoxMarker(marker: AnnotationMarker): void {
    // Marker coordinates are relative to image area, add padding offset
    const x = this.imagePadding + (marker.x / 100) * this.imageDisplayWidth;
    const y = this.imagePadding + (marker.y / 100) * this.imageDisplayHeight;
    const w = ((marker.width || 10) / 100) * this.imageDisplayWidth;
    const h = ((marker.height || 10) / 100) * this.imageDisplayHeight;

    this.ctx.save();

    // Draw rectangle with shadow
    this.ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
    this.ctx.shadowBlur = 4;
    this.ctx.shadowOffsetX = 2;
    this.ctx.shadowOffsetY = 2;

    // Transparent rectangle with border
    this.ctx.strokeStyle = marker.color;
    this.ctx.lineWidth = 3;
    this.ctx.strokeRect(x, y, w, h);

    // Reset shadow for the number badge
    this.ctx.shadowColor = 'transparent';

    // Draw number badge in top-left corner (inside the box)
    const badgeRadius = 12;
    const badgeX = x + badgeRadius + 4;
    const badgeY = y + badgeRadius + 4;

    this.ctx.beginPath();
    this.ctx.arc(badgeX, badgeY, badgeRadius, 0, Math.PI * 2);
    this.ctx.fillStyle = marker.color;
    this.ctx.fill();
    this.ctx.strokeStyle = '#FFFFFF';
    this.ctx.lineWidth = 2;
    this.ctx.stroke();

    // Draw number
    this.ctx.fillStyle = '#FFFFFF';
    this.ctx.font = 'bold 12px Arial';
    this.ctx.textAlign = 'center';
    this.ctx.textBaseline = 'middle';
    this.ctx.fillText(marker.id.toString(), badgeX, badgeY);

    // Draw resize handles if not in export mode
    this.drawResizeHandles(marker);

    this.ctx.restore();
  }

  private drawResizeHandles(marker: AnnotationMarker): void {
    if (marker.type !== 'box' || this.isExportMode) return;

    // Marker coordinates are relative to image area, add padding offset
    const x = this.imagePadding + (marker.x / 100) * this.imageDisplayWidth;
    const y = this.imagePadding + (marker.y / 100) * this.imageDisplayHeight;
    const w = ((marker.width || 10) / 100) * this.imageDisplayWidth;
    const h = ((marker.height || 10) / 100) * this.imageDisplayHeight;
    const hs = this.HANDLE_SIZE;

    this.ctx.fillStyle = '#FFFFFF';
    this.ctx.strokeStyle = marker.color;
    this.ctx.lineWidth = 2;

    // Corner handles
    const handles = [
      { x: x - hs/2, y: y - hs/2 },           // nw
      { x: x + w - hs/2, y: y - hs/2 },       // ne
      { x: x - hs/2, y: y + h - hs/2 },       // sw
      { x: x + w - hs/2, y: y + h - hs/2 },   // se
      // Edge handles
      { x: x + w/2 - hs/2, y: y - hs/2 },     // n
      { x: x + w/2 - hs/2, y: y + h - hs/2 }, // s
      { x: x - hs/2, y: y + h/2 - hs/2 },     // w
      { x: x + w - hs/2, y: y + h/2 - hs/2 }, // e
    ];

    handles.forEach(handle => {
      this.ctx.fillRect(handle.x, handle.y, hs, hs);
      this.ctx.strokeRect(handle.x, handle.y, hs, hs);
    });
  }

  onCanvasClick(event: MouseEvent): void {
    // This is now handled by mousedown/mouseup for better control
    // Kept for backward compatibility but does nothing
  }

  onMouseDown(event: MouseEvent): void {
    if (!this.imageLoaded) return;

    const { xPercent, yPercent } = this.getMousePosition(event);

    // Check if clicking on a resize handle (only for boxes)
    for (let i = this.markers.length - 1; i >= 0; i--) {
      const marker = this.markers[i];
      if (marker.type === 'box') {
        const handle = this.hitTestResizeHandle(xPercent, yPercent, marker);
        if (handle) {
          this.isResizing = true;
          this.resizeHandle = handle;
          this.draggedMarker = marker;
          return;
        }
      }
    }

    // Check if clicking on an existing marker
    const hitMarker = this.hitTestMarker(xPercent, yPercent);
    if (hitMarker) {
      this.isDragging = true;
      this.draggedMarker = hitMarker;
      this.dragOffset = {
        x: xPercent - hitMarker.x,
        y: yPercent - hitMarker.y
      };
      return;
    }

    // Create new marker based on current tool
    if (this.currentTool === 'ball') {
      this.createBallMarker(xPercent, yPercent);
    } else {
      // Start drawing box
      this.isDrawingBox = true;
      this.boxStartPoint = { x: xPercent, y: yPercent };
    }
  }

  onMouseMove(event: MouseEvent): void {
    if (!this.imageLoaded) return;

    const { xPercent, yPercent } = this.getMousePosition(event);

    // Update cursor based on what's under the mouse
    this.updateCursor(xPercent, yPercent);

    if (this.isDragging && this.draggedMarker) {
      // Move marker
      this.draggedMarker.x = xPercent - this.dragOffset.x;
      this.draggedMarker.y = yPercent - this.dragOffset.y;
      this.redraw();
    } else if (this.isResizing && this.draggedMarker && this.resizeHandle) {
      // Resize box
      this.resizeBox(this.draggedMarker, xPercent, yPercent, this.resizeHandle);
      this.redraw();
    } else if (this.isDrawingBox && this.boxStartPoint) {
      // Preview box
      const x = Math.min(this.boxStartPoint.x, xPercent);
      const y = Math.min(this.boxStartPoint.y, yPercent);
      const width = Math.abs(xPercent - this.boxStartPoint.x);
      const height = Math.abs(yPercent - this.boxStartPoint.y);
      this.boxPreview = { x, y, width, height };
      this.redraw();
      this.drawBoxPreview();
    }
  }

  onMouseUp(event: MouseEvent): void {
    if (this.isDrawingBox && this.boxStartPoint) {
      const { xPercent, yPercent } = this.getMousePosition(event);
      const x = Math.min(this.boxStartPoint.x, xPercent);
      const y = Math.min(this.boxStartPoint.y, yPercent);
      const width = Math.abs(xPercent - this.boxStartPoint.x);
      const height = Math.abs(yPercent - this.boxStartPoint.y);

      // Only create box if it has minimum size
      if (width > 2 && height > 2) {
        this.createBoxMarker(x, y, width, height);
      }
    }

    // Emit changes if we were dragging or resizing
    if ((this.isDragging || this.isResizing) && this.draggedMarker) {
      this.markersChanged.emit([...this.markers]);
    }

    // Reset all states
    this.isDragging = false;
    this.isResizing = false;
    this.isDrawingBox = false;
    this.draggedMarker = null;
    this.resizeHandle = null;
    this.boxStartPoint = null;
    this.boxPreview = null;
    this.redraw();
  }

  private getMousePosition(event: MouseEvent): { xPercent: number, yPercent: number } {
    const rect = this.canvas.getBoundingClientRect();
    // Account for CSS scaling (canvas may be displayed at different size than internal resolution)
    const scaleX = this.canvas.width / rect.width;
    const scaleY = this.canvas.height / rect.height;
    const clickX = (event.clientX - rect.left) * scaleX;
    const clickY = (event.clientY - rect.top) * scaleY;

    // Convert to percentage relative to image area (not canvas area)
    return {
      xPercent: ((clickX - this.imagePadding) / this.imageDisplayWidth) * 100,
      yPercent: ((clickY - this.imagePadding) / this.imageDisplayHeight) * 100
    };
  }

  private hitTestMarker(xPercent: number, yPercent: number): AnnotationMarker | null {
    // Check in reverse order (last markers are on top)
    for (let i = this.markers.length - 1; i >= 0; i--) {
      const m = this.markers[i];
      if (m.type === 'ball') {
        // Test circle
        const dist = Math.sqrt(Math.pow(xPercent - m.x, 2) + Math.pow(yPercent - m.y, 2));
        if (dist <= this.BALL_HIT_RADIUS_PERCENT) return m;
      } else {
        // Test rectangle
        const w = m.width || 10;
        const h = m.height || 10;
        if (xPercent >= m.x && xPercent <= m.x + w &&
            yPercent >= m.y && yPercent <= m.y + h) {
          return m;
        }
      }
    }
    return null;
  }

  private hitTestResizeHandle(xPercent: number, yPercent: number, box: AnnotationMarker): ResizeHandle | null {
    if (box.type !== 'box') return null;

    const w = box.width || 10;
    const h = box.height || 10;
    const tolerance = 1.5; // percentage tolerance for hit testing

    // Check all 8 handles
    const handles: { handle: ResizeHandle, x: number, y: number }[] = [
      { handle: 'nw', x: box.x, y: box.y },
      { handle: 'ne', x: box.x + w, y: box.y },
      { handle: 'sw', x: box.x, y: box.y + h },
      { handle: 'se', x: box.x + w, y: box.y + h },
      { handle: 'n', x: box.x + w/2, y: box.y },
      { handle: 's', x: box.x + w/2, y: box.y + h },
      { handle: 'w', x: box.x, y: box.y + h/2 },
      { handle: 'e', x: box.x + w, y: box.y + h/2 },
    ];

    for (const { handle, x, y } of handles) {
      if (Math.abs(xPercent - x) <= tolerance && Math.abs(yPercent - y) <= tolerance) {
        return handle;
      }
    }
    return null;
  }

  private resizeBox(marker: AnnotationMarker, xPercent: number, yPercent: number, handle: ResizeHandle): void {
    const minSize = 3; // minimum size in percentage
    const w = marker.width || 10;
    const h = marker.height || 10;

    switch (handle) {
      case 'nw':
        const newW_nw = marker.x + w - xPercent;
        const newH_nw = marker.y + h - yPercent;
        if (newW_nw >= minSize && newH_nw >= minSize) {
          marker.width = newW_nw;
          marker.height = newH_nw;
          marker.x = xPercent;
          marker.y = yPercent;
        }
        break;
      case 'ne':
        const newW_ne = xPercent - marker.x;
        const newH_ne = marker.y + h - yPercent;
        if (newW_ne >= minSize && newH_ne >= minSize) {
          marker.width = newW_ne;
          marker.height = newH_ne;
          marker.y = yPercent;
        }
        break;
      case 'sw':
        const newW_sw = marker.x + w - xPercent;
        const newH_sw = yPercent - marker.y;
        if (newW_sw >= minSize && newH_sw >= minSize) {
          marker.width = newW_sw;
          marker.height = newH_sw;
          marker.x = xPercent;
        }
        break;
      case 'se':
        const newW_se = xPercent - marker.x;
        const newH_se = yPercent - marker.y;
        if (newW_se >= minSize) marker.width = newW_se;
        if (newH_se >= minSize) marker.height = newH_se;
        break;
      case 'n':
        const newH_n = marker.y + h - yPercent;
        if (newH_n >= minSize) {
          marker.height = newH_n;
          marker.y = yPercent;
        }
        break;
      case 's':
        const newH_s = yPercent - marker.y;
        if (newH_s >= minSize) marker.height = newH_s;
        break;
      case 'w':
        const newW_w = marker.x + w - xPercent;
        if (newW_w >= minSize) {
          marker.width = newW_w;
          marker.x = xPercent;
        }
        break;
      case 'e':
        const newW_e = xPercent - marker.x;
        if (newW_e >= minSize) marker.width = newW_e;
        break;
    }
  }

  private updateCursor(xPercent: number, yPercent: number): void {
    // Check for resize handles first
    for (let i = this.markers.length - 1; i >= 0; i--) {
      const marker = this.markers[i];
      if (marker.type === 'box') {
        const handle = this.hitTestResizeHandle(xPercent, yPercent, marker);
        if (handle) {
          this.currentCursor = this.getCursorForHandle(handle);
          return;
        }
      }
    }

    // Check for markers (move cursor)
    const hitMarker = this.hitTestMarker(xPercent, yPercent);
    if (hitMarker) {
      this.currentCursor = 'move';
      return;
    }

    // Default crosshair for creating
    this.currentCursor = 'crosshair';
  }

  private getCursorForHandle(handle: ResizeHandle): string {
    switch (handle) {
      case 'nw':
      case 'se':
        return 'nwse-resize';
      case 'ne':
      case 'sw':
        return 'nesw-resize';
      case 'n':
      case 's':
        return 'ns-resize';
      case 'e':
      case 'w':
        return 'ew-resize';
      default:
        return 'default';
    }
  }

  private createBallMarker(xPercent: number, yPercent: number): void {
    const newMarker: AnnotationMarker = {
      id: this.markers.length + 1,
      type: 'ball',
      x: xPercent,
      y: yPercent,
      color: this.markerColor
    };

    this.markers.push(newMarker);
    this.redraw();
    this.markersChanged.emit([...this.markers]);
    console.log('[ImageAnnotationCanvas] Ball marker added:', newMarker);
  }

  private createBoxMarker(x: number, y: number, width: number, height: number): void {
    const newMarker: AnnotationMarker = {
      id: this.markers.length + 1,
      type: 'box',
      x,
      y,
      width,
      height,
      color: this.markerColor
    };

    this.markers.push(newMarker);
    this.redraw();
    this.markersChanged.emit([...this.markers]);
    console.log('[ImageAnnotationCanvas] Box marker added:', newMarker);
  }

  private drawBoxPreview(): void {
    if (!this.boxPreview) return;

    // Box preview coordinates are relative to image area, add padding offset
    const x = this.imagePadding + (this.boxPreview.x / 100) * this.imageDisplayWidth;
    const y = this.imagePadding + (this.boxPreview.y / 100) * this.imageDisplayHeight;
    const w = (this.boxPreview.width / 100) * this.imageDisplayWidth;
    const h = (this.boxPreview.height / 100) * this.imageDisplayHeight;

    this.ctx.save();
    this.ctx.strokeStyle = this.markerColor;
    this.ctx.lineWidth = 2;
    this.ctx.setLineDash([5, 5]);
    this.ctx.strokeRect(x, y, w, h);
    this.ctx.restore();
  }

  setTool(tool: MarkerType): void {
    this.currentTool = tool;
    console.log('[ImageAnnotationCanvas] Tool changed to:', tool);
  }

  removeLastMarker(): void {
    if (this.markers.length > 0) {
      this.markers.pop();
      this.redraw();
      this.markersChanged.emit([...this.markers]);
      console.log('[ImageAnnotationCanvas] Last marker removed');
    }
  }

  clearMarkers(): void {
    this.markers = [];
    this.redraw();
    this.markersChanged.emit([]);
    console.log('[ImageAnnotationCanvas] All markers cleared');
  }

  setMarkerColor(color: string): void {
    this.markerColor = color;
    // Note: This only affects NEW markers, existing markers keep their color
  }

  onMarkerRadiusChanged(value: number): void {
    console.log('[ImageAnnotationCanvas] Marker radius changed to:', value);
    this.markerRadius = Math.max(this.MIN_MARKER_RADIUS,
                                 Math.min(this.MAX_MARKER_RADIUS, value));
    this.redraw();
  }

  onPaddingChanged(value: number): void {
    console.log('[ImageAnnotationCanvas] Padding changed to:', value);
    this.imagePadding = Math.max(0, Math.min(this.MAX_IMAGE_PADDING, value));
    this.resizeCanvas();
    this.redraw();
  }

  getMarkerColor(markerId: number): string {
    const marker = this.markers.find(m => m.id === markerId);
    return marker?.color || this.markerColor;
  }

  getMarkers(): AnnotationMarker[] {
    return [...this.markers];
  }

  /**
   * Export the canvas as a Blob (PNG format) at FULL ORIGINAL resolution
   * Creates a temporary canvas at the original image size for high-quality export
   */
  async exportAsBlob(): Promise<Blob> {
    if (!this.image) {
      throw new Error('No image loaded');
    }

    // Calculate scale factor: how much bigger is the original image vs the displayed version
    const scaleX = this.image.width / this.imageDisplayWidth;
    const scaleY = this.image.height / this.imageDisplayHeight;

    // Scale padding to original image pixels
    const exportPaddingX = Math.round(this.imagePadding * scaleX);
    const exportPaddingY = Math.round(this.imagePadding * scaleY);

    // Create export canvas at original image resolution (plus padding)
    const exportCanvas = document.createElement('canvas');
    exportCanvas.width = this.image.width + (exportPaddingX * 2);
    exportCanvas.height = this.image.height + (exportPaddingY * 2);
    const exportCtx = exportCanvas.getContext('2d')!;

    console.log('[ImageAnnotationCanvas] Exporting at resolution:', exportCanvas.width, 'x', exportCanvas.height);

    // Fill padding area with background color
    if (this.imagePadding > 0) {
      exportCtx.fillStyle = '#f0f0f0';
      exportCtx.fillRect(0, 0, exportCanvas.width, exportCanvas.height);
    }

    // Draw original image at full resolution (centered in padding)
    exportCtx.drawImage(this.image, exportPaddingX, exportPaddingY);

    // Draw markers scaled to original resolution
    this.markers.forEach(marker => {
      this.drawMarkerOnExportCanvas(exportCtx, marker, exportPaddingX, exportPaddingY, scaleX, scaleY);
    });

    return new Promise((resolve, reject) => {
      exportCanvas.toBlob((blob) => {
        if (blob) {
          console.log('[ImageAnnotationCanvas] Export complete, blob size:', blob.size);
          resolve(blob);
        } else {
          reject(new Error('Failed to export canvas as blob'));
        }
      }, 'image/png');
    });
  }

  /**
   * Draw a marker on the export canvas at the scaled coordinates
   */
  private drawMarkerOnExportCanvas(
    ctx: CanvasRenderingContext2D,
    marker: AnnotationMarker,
    paddingX: number,
    paddingY: number,
    scaleX: number,
    scaleY: number
  ): void {
    if (marker.type === 'ball') {
      this.drawBallMarkerOnExportCanvas(ctx, marker, paddingX, paddingY, scaleX, scaleY);
    } else {
      this.drawBoxMarkerOnExportCanvas(ctx, marker, paddingX, paddingY, scaleX, scaleY);
    }
  }

  private drawBallMarkerOnExportCanvas(
    ctx: CanvasRenderingContext2D,
    marker: AnnotationMarker,
    paddingX: number,
    paddingY: number,
    scaleX: number,
    scaleY: number
  ): void {
    // Marker coordinates are percentage of image area
    const x = paddingX + (marker.x / 100) * this.image!.width;
    const y = paddingY + (marker.y / 100) * this.image!.height;

    // Scale marker radius proportionally
    const scaledRadius = this.markerRadius * Math.max(scaleX, scaleY);
    const scaledFontSize = Math.round(14 * Math.max(scaleX, scaleY));

    ctx.save();
    ctx.shadowColor = 'rgba(0, 0, 0, 0.4)';
    ctx.shadowBlur = 4 * Math.max(scaleX, scaleY);
    ctx.shadowOffsetX = 2 * scaleX;
    ctx.shadowOffsetY = 2 * scaleY;

    ctx.beginPath();
    ctx.arc(x, y, scaledRadius, 0, Math.PI * 2);
    ctx.fillStyle = marker.color;
    ctx.fill();

    ctx.shadowColor = 'transparent';
    ctx.strokeStyle = '#FFFFFF';
    ctx.lineWidth = 2 * Math.max(scaleX, scaleY);
    ctx.stroke();

    ctx.fillStyle = '#FFFFFF';
    ctx.font = `bold ${scaledFontSize}px Arial`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(marker.id.toString(), x, y);

    ctx.restore();
  }

  private drawBoxMarkerOnExportCanvas(
    ctx: CanvasRenderingContext2D,
    marker: AnnotationMarker,
    paddingX: number,
    paddingY: number,
    scaleX: number,
    scaleY: number
  ): void {
    // Marker coordinates are percentage of image area
    const x = paddingX + (marker.x / 100) * this.image!.width;
    const y = paddingY + (marker.y / 100) * this.image!.height;
    const w = ((marker.width || 10) / 100) * this.image!.width;
    const h = ((marker.height || 10) / 100) * this.image!.height;

    ctx.save();

    ctx.shadowColor = 'rgba(0, 0, 0, 0.3)';
    ctx.shadowBlur = 4 * Math.max(scaleX, scaleY);
    ctx.shadowOffsetX = 2 * scaleX;
    ctx.shadowOffsetY = 2 * scaleY;

    ctx.strokeStyle = marker.color;
    ctx.lineWidth = 3 * Math.max(scaleX, scaleY);
    ctx.strokeRect(x, y, w, h);

    ctx.shadowColor = 'transparent';

    // Scale badge radius and font
    const badgeRadius = 12 * Math.max(scaleX, scaleY);
    const badgeX = x + badgeRadius + (4 * scaleX);
    const badgeY = y + badgeRadius + (4 * scaleY);
    const scaledFontSize = Math.round(12 * Math.max(scaleX, scaleY));

    ctx.beginPath();
    ctx.arc(badgeX, badgeY, badgeRadius, 0, Math.PI * 2);
    ctx.fillStyle = marker.color;
    ctx.fill();
    ctx.strokeStyle = '#FFFFFF';
    ctx.lineWidth = 2 * Math.max(scaleX, scaleY);
    ctx.stroke();

    ctx.fillStyle = '#FFFFFF';
    ctx.font = `bold ${scaledFontSize}px Arial`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(marker.id.toString(), badgeX, badgeY);

    ctx.restore();
  }

  /**
   * Get the original image blob (without annotations)
   */
  getOriginalBlob(): Blob {
    return this.imageBlob;
  }
}
