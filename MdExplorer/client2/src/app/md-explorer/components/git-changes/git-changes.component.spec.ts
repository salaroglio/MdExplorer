import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GitChangesComponent } from './git-changes.component';

describe('GitChangesComponent', () => {
  let component: GitChangesComponent;
  let fixture: ComponentFixture<GitChangesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GitChangesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GitChangesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
