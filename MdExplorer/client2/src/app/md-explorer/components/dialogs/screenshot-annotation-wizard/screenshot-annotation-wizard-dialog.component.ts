import { Component, Inject, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageAnnotationCanvasComponent, AnnotationMarker } from '../../image-annotation-canvas/image-annotation-canvas.component';
import { MdFileService } from '../../../services/md-file.service';
import { TranslateService } from '@ngx-translate/core';

export interface MarkerDescription {
  markerId: number;
  text: string;
}

export interface WizardDialogData {
  imageBlob: Blob;
  documentPath: string;
  connectionId: string;
}

@Component({
  selector: 'app-screenshot-annotation-wizard-dialog',
  templateUrl: './screenshot-annotation-wizard-dialog.component.html',
  styleUrls: ['./screenshot-annotation-wizard-dialog.component.scss']
})
export class ScreenshotAnnotationWizardDialogComponent implements OnInit {
  @ViewChild('annotationCanvas', { static: false }) annotationCanvas!: ImageAnnotationCanvasComponent;

  // Wizard state
  currentStep: number = 0;
  isLoading: boolean = false;

  // Step 0: Annotation data
  markers: AnnotationMarker[] = [];
  imageName: string = 'screenshot';

  // Step 1: Descriptions
  descriptions: MarkerDescription[] = [];

  // Reference image for step 1
  annotatedImageUrl: SafeUrl | null = null;
  private annotatedImageBlobUrl: string | null = null;

  // Cached blobs for saving (must be captured before canvas is hidden)
  private cachedOriginalBlob: Blob | null = null;
  private cachedAnnotatedBlob: Blob | null = null;

  constructor(
    public dialogRef: MatDialogRef<ScreenshotAnnotationWizardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: WizardDialogData,
    private mdFileService: MdFileService,
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    console.log('[ScreenshotAnnotationWizard] Initialized with data:', {
      documentPath: this.data.documentPath,
      connectionId: this.data.connectionId,
      imageBlobSize: this.data.imageBlob?.size
    });
  }

  /**
   * Handle markers changed event from canvas
   */
  onMarkersChanged(markers: AnnotationMarker[]): void {
    this.markers = markers;
    console.log('[ScreenshotAnnotationWizard] Markers updated:', markers.length);
  }

  /**
   * Check if we can proceed to step 1
   * Now always returns true - users can proceed without markers
   */
  canProceedToStep1(): boolean {
    return true;
  }

  /**
   * Move to step 1 (descriptions)
   */
  async goToStep1(): Promise<void> {
    // Export and cache both images BEFORE hiding the canvas
    try {
      // Cache original blob
      this.cachedOriginalBlob = this.annotationCanvas.getOriginalBlob();

      // Cache annotated blob (must be done while canvas is visible!)
      this.cachedAnnotatedBlob = await this.annotationCanvas.exportAsBlob();

      // Create URL for preview
      this.annotatedImageBlobUrl = URL.createObjectURL(this.cachedAnnotatedBlob);
      this.annotatedImageUrl = this.sanitizer.bypassSecurityTrustUrl(this.annotatedImageBlobUrl);
      console.log('[ScreenshotAnnotationWizard] Blobs cached and URL created:', this.annotatedImageBlobUrl);
    } catch (err) {
      console.error('[ScreenshotAnnotationWizard] Failed to export annotated image:', err);
      this.snackBar.open(this.translate.instant('SCREENSHOT.EXPORT_ERROR'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    // Initialize descriptions array
    this.descriptions = this.markers.map(m => ({
      markerId: m.id,
      text: ''
    }));

    this.currentStep = 1;
    console.log('[ScreenshotAnnotationWizard] Moved to step 1 with', this.markers.length, 'markers');
  }

  /**
   * Go back to step 0
   */
  goToStep0(): void {
    this.currentStep = 0;
    // Clean up annotated image URL
    if (this.annotatedImageBlobUrl) {
      URL.revokeObjectURL(this.annotatedImageBlobUrl);
      this.annotatedImageBlobUrl = null;
      this.annotatedImageUrl = null;
    }
    // Clear cached blobs so they get recaptured if user modifies markers
    this.cachedOriginalBlob = null;
    this.cachedAnnotatedBlob = null;
  }

  /**
   * Check if all descriptions are filled
   * Returns true if no markers exist (nothing to describe)
   */
  allDescriptionsFilled(): boolean {
    if (this.descriptions.length === 0) return true;
    return this.descriptions.every(d => d.text && d.text.trim().length > 0);
  }

  /**
   * Get the color of a specific marker
   */
  getMarkerColor(markerId: number): string {
    return this.annotationCanvas?.getMarkerColor(markerId) || '#FF4444';
  }

  /**
   * Complete the wizard and save
   */
  async complete(): Promise<void> {
    if (!this.allDescriptionsFilled()) {
      this.snackBar.open(this.translate.instant('SCREENSHOT.ENTER_MARKER_DESC'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }

    this.isLoading = true;

    try {
      // Use cached blobs (captured when moving to step 1, before canvas was hidden)
      if (!this.cachedOriginalBlob || !this.cachedAnnotatedBlob) {
        throw new Error('Image blobs not cached. Please go back and try again.');
      }

      // Create FormData
      const formData = new FormData();
      formData.append('OriginalImage', this.cachedOriginalBlob, 'original.png');
      formData.append('AnnotatedImage', this.cachedAnnotatedBlob, 'annotated.png');
      formData.append('DocumentPath', this.data.documentPath);
      formData.append('ImageName', this.imageName);
      formData.append('DescriptionsJson', JSON.stringify(this.descriptions));
      formData.append('ConnectionId', this.data.connectionId || '');

      // Call backend service
      this.mdFileService.saveAnnotatedScreenshot(formData).subscribe({
        next: (response) => {
          this.isLoading = false;

          if (response.success) {
            console.log('[ScreenshotAnnotationWizard] Screenshot saved successfully');
            this.snackBar.open(this.translate.instant('SCREENSHOT.SAVE_SUCCESS'), 'OK', {
              duration: 3000,
              verticalPosition: 'top',
              panelClass: ['success-snackbar']
            });
            this.dialogRef.close(true);
          } else {
            console.error('[ScreenshotAnnotationWizard] Save failed:', response.errorMessage);
            this.snackBar.open(`Error: ${response.errorMessage}`, 'OK', {
              duration: 5000,
              verticalPosition: 'top',
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          this.isLoading = false;
          console.error('[ScreenshotAnnotationWizard] Save error:', error);
          const errorMessage = error?.error?.errorMessage || error?.message || 'Unknown error';
          this.snackBar.open(`Error: ${errorMessage}`, 'OK', {
            duration: 5000,
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          });
        }
      });
    } catch (err) {
      this.isLoading = false;
      console.error('[ScreenshotAnnotationWizard] Error preparing data:', err);
      this.snackBar.open(this.translate.instant('SCREENSHOT.PREPARE_ERROR'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
    }
  }

  /**
   * Cancel wizard
   */
  cancel(): void {
    // Clean up
    if (this.annotatedImageBlobUrl) {
      URL.revokeObjectURL(this.annotatedImageBlobUrl);
    }
    this.dialogRef.close(false);
  }

  ngOnDestroy(): void {
    // Clean up URL object
    if (this.annotatedImageBlobUrl) {
      URL.revokeObjectURL(this.annotatedImageBlobUrl);
    }
  }
}
