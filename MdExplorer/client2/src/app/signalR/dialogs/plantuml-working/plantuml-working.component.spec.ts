import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlantumlWorkingComponent } from './plantuml-working.component';

describe('PlantumlWorkingComponent', () => {
  let component: PlantumlWorkingComponent;
  let fixture: ComponentFixture<PlantumlWorkingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PlantumlWorkingComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PlantumlWorkingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
