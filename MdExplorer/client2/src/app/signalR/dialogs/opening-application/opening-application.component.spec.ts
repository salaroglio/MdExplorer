import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpeningApplicationComponent } from './opening-application.component';

describe('OpeningApplicationComponent', () => {
  let component: OpeningApplicationComponent;
  let fixture: ComponentFixture<OpeningApplicationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OpeningApplicationComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OpeningApplicationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
