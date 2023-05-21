import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PublishMdTreeComponent } from './publish-md-tree.component';

describe('PublishMdTreeComponent', () => {
  let component: PublishMdTreeComponent;
  let fixture: ComponentFixture<PublishMdTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PublishMdTreeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PublishMdTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
