import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GitlabSettingsComponent } from './gitlab-settings.component';

describe('GitlabSettingsComponent', () => {
  let component: GitlabSettingsComponent;
  let fixture: ComponentFixture<GitlabSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GitlabSettingsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GitlabSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
