import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenRecentComponent } from './open-recent.component';

describe('OpenRecentComponent', () => {
  let component: OpenRecentComponent;
  let fixture: ComponentFixture<OpenRecentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OpenRecentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OpenRecentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
