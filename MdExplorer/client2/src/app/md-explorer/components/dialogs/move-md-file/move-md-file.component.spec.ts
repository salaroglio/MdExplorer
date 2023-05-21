import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MoveMdFileComponent } from './move-md-file.component';

describe('MoveMdFileComponent', () => {
  let component: MoveMdFileComponent;
  let fixture: ComponentFixture<MoveMdFileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MoveMdFileComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MoveMdFileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
