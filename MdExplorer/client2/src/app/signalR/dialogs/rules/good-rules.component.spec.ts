import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoodRulesComponent } from './good-rules.component';

describe('RulesComponent', () => {
  let component: GoodRulesComponent;
  let fixture: ComponentFixture<GoodRulesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GoodRulesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GoodRulesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
