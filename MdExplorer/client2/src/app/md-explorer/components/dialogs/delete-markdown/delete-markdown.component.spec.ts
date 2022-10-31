import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteMarkdownComponent } from './delete-markdown.component';

describe('DeleteMarkdownComponent', () => {
  let component: DeleteMarkdownComponent;
  let fixture: ComponentFixture<DeleteMarkdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeleteMarkdownComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DeleteMarkdownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
