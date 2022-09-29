import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewDirectoryComponent } from './new-directory.component';

describe('NewDirectoryComponent', () => {
  let component: NewDirectoryComponent;
  let fixture: ComponentFixture<NewDirectoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NewDirectoryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewDirectoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
