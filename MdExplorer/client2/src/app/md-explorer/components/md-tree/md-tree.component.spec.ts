import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MdTreeComponent } from './md-tree.component';

describe('MdTreeComponent', () => {
  let component: MdTreeComponent;
  let fixture: ComponentFixture<MdTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MdTreeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MdTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
