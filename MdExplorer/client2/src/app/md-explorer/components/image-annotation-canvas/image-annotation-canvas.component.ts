import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';

export interface AnnotationMarker {
  id: number;
  x: number;  // percentage relative to image width
  y: number;  // percentage relative to image height
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

  private canvas!: HTMLCanvasElement;
  private ctx!: CanvasRenderingContext2D;
  private image: HTMLImageElement | null = null;
  private imageLoaded = false;
  private markerRadius = 16;
  private resizeObserver?: any; // ResizeObserver

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

    const container = this.containerRef.nativeElement;
    const containerWidth = container.clientWidth;
    const containerHeight = container.clientHeight;

    // Calculate aspect ratio to fit image in container
    const imageRatio = this.image.width / this.image.height;
    const containerRatio = containerWidth / containerHeight;

    let canvasWidth: number;
    let canvasHeight: number;

    if (imageRatio > containerRatio) {
      // Image is wider - fit to width
      canvasWidth = containerWidth;
      canvasHeight = containerWidth / imageRatio;
    } else {
      // Image is taller - fit to height
      canvasHeight = containerHeight;
      canvasWidth = containerHeight * imageRatio;
    }

    this.canvas.width = canvasWidth;
    this.canvas.height = canvasHeight;

    console.log('[ImageAnnotationCanvas] Canvas resized to:', canvasWidth, 'x', canvasHeight);
  }

  private redraw(): void {
    if (!this.ctx || !this.image || !this.imageLoaded) return;

    // Clear canvas
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    // Draw image scaled to canvas
    this.ctx.drawImage(this.image, 0, 0, this.canvas.width, this.canvas.height);

    // Draw markers
    this.markers.forEach(marker => {
      this.drawMarker(marker);
    });
  }

  private drawMarker(marker: AnnotationMarker): void {
    const x = (marker.x / 100) * this.canvas.width;
    const y = (marker.y / 100) * this.canvas.height;

    // Draw circle with shadow
    this.ctx.save();
    this.ctx.shadowColor = 'rgba(0, 0, 0, 0.4)';
    this.ctx.shadowBlur = 4;
    this.ctx.shadowOffsetX = 2;
    this.ctx.shadowOffsetY = 2;

    this.ctx.beginPath();
    this.ctx.arc(x, y, this.markerRadius, 0, Math.PI * 2);
    this.ctx.fillStyle = this.markerColor;
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

  onCanvasClick(event: MouseEvent): void {
    if (!this.imageLoaded) return;

    const rect = this.canvas.getBoundingClientRect();
    const clickX = event.clientX - rect.left;
    const clickY = event.clientY - rect.top;

    // Convert to percentage
    const xPercent = (clickX / this.canvas.width) * 100;
    const yPercent = (clickY / this.canvas.height) * 100;

    // Create new marker
    const newMarker: AnnotationMarker = {
      id: this.markers.length + 1,
      x: xPercent,
      y: yPercent
    };

    this.markers.push(newMarker);
    this.redraw();
    this.markersChanged.emit([...this.markers]);

    console.log('[ImageAnnotationCanvas] Marker added:', newMarker);
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
    this.redraw();
  }

  getMarkers(): AnnotationMarker[] {
    return [...this.markers];
  }

  /**
   * Export the canvas as a Blob (PNG format)
   */
  async exportAsBlob(): Promise<Blob> {
    return new Promise((resolve, reject) => {
      this.canvas.toBlob((blob) => {
        if (blob) {
          resolve(blob);
        } else {
          reject(new Error('Failed to export canvas as blob'));
        }
      }, 'image/png');
    });
  }

  /**
   * Get the original image blob (without annotations)
   */
  getOriginalBlob(): Blob {
    return this.imageBlob;
  }
}
