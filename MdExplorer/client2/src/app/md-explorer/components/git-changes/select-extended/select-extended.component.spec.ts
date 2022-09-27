import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectExtendedComponent } from './select-extended.component';

describe('SelectExtendedComponent', () => {
  let component: SelectExtendedComponent;
  let fixture: ComponentFixture<SelectExtendedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SelectExtendedComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SelectExtendedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
