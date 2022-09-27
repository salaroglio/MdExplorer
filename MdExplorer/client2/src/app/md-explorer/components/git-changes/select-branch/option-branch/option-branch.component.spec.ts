import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OptionBranchComponent } from './option-branch.component';

describe('OptionBranchComponent', () => {
  let component: OptionBranchComponent;
  let fixture: ComponentFixture<OptionBranchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OptionBranchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OptionBranchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
