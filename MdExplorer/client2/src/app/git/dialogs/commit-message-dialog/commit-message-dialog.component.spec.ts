import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatLegacyDialogRef as MatDialogRef, MAT_LEGACY_DIALOG_DATA as MAT_DIALOG_DATA } from '@angular/material/legacy-dialog';
import { FormsModule } from '@angular/forms';
import { MatLegacyFormFieldModule as MatFormFieldModule } from '@angular/material/legacy-form-field';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { CommitMessageDialogComponent } from './commit-message-dialog.component';

describe('CommitMessageDialogComponent', () => {
  let component: CommitMessageDialogComponent;
  let fixture: ComponentFixture<CommitMessageDialogComponent>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<CommitMessageDialogComponent>>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      declarations: [ CommitMessageDialogComponent ],
      imports: [
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatDialogModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { defaultMessage: 'Test message' } }
      ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CommitMessageDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default message', () => {
    expect(component.commitMessage).toBe('Test message');
  });

  it('should close dialog with null on cancel', () => {
    component.onCancel();
    expect(mockDialogRef.close).toHaveBeenCalledWith(null);
  });

  it('should close dialog with trimmed message on confirm', () => {
    component.commitMessage = '  New commit message  ';
    component.onConfirm();
    expect(mockDialogRef.close).toHaveBeenCalledWith('New commit message');
  });

  it('should not close dialog if message is empty', () => {
    component.commitMessage = '   ';
    component.onConfirm();
    expect(mockDialogRef.close).not.toHaveBeenCalled();
  });
});