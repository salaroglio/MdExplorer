import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddNewFileToMDEComponent } from './add-new-file-to-mde.component';

describe('AddNewFileToMDEComponent', () => {
  let component: AddNewFileToMDEComponent;
  let fixture: ComponentFixture<AddNewFileToMDEComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AddNewFileToMDEComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddNewFileToMDEComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
