import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CopyFromClipboardComponent } from './copy-from-clipboard.component';

describe('CopyFromClipboardComponent', () => {
  let component: CopyFromClipboardComponent;
  let fixture: ComponentFixture<CopyFromClipboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CopyFromClipboardComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CopyFromClipboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
