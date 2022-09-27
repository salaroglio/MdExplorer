import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OptionExtendedComponent } from './option-extended.component';

describe('OptionExtendedComponent', () => {
  let component: OptionExtendedComponent;
  let fixture: ComponentFixture<OptionExtendedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OptionExtendedComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OptionExtendedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
