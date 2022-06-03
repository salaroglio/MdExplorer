import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewMarkdownComponent } from './new-markdown.component';

describe('NewMarkdownComponent', () => {
  let component: NewMarkdownComponent;
  let fixture: ComponentFixture<NewMarkdownComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NewMarkdownComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewMarkdownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
