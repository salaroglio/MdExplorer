import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentSettingsComponent } from './document-settings.component';

describe('DocumentSettingsComponent', () => {
  let component: DocumentSettingsComponent;
  let fixture: ComponentFixture<DocumentSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DocumentSettingsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
