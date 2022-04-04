import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TreeDynamicExampleComponent } from './tree-dynamic-example.component';

describe('TreeDynamicExampleComponent', () => {
  let component: TreeDynamicExampleComponent;
  let fixture: ComponentFixture<TreeDynamicExampleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TreeDynamicExampleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TreeDynamicExampleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
