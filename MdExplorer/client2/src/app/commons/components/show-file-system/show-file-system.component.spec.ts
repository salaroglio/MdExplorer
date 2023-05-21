import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowFileSystemComponent } from './show-file-system.component';

describe('ShowFileSystemComponent', () => {
  let component: ShowFileSystemComponent;
  let fixture: ComponentFixture<ShowFileSystemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ShowFileSystemComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ShowFileSystemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
