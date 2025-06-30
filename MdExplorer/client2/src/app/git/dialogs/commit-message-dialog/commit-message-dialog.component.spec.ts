import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
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