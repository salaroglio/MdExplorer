import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CloneProjectComponent } from './clone-project.component';

describe('CloneProjectComponent', () => {
  let component: CloneProjectComponent;
  let fixture: ComponentFixture<CloneProjectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CloneProjectComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CloneProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
