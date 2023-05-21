import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GitMessagesComponent } from './git-messages.component';

describe('GitMessagesComponent', () => {
  let component: GitMessagesComponent;
  let fixture: ComponentFixture<GitMessagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GitMessagesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GitMessagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
