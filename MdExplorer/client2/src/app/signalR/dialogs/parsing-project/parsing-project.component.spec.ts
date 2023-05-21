import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParsingProjectComponent } from './parsing-project.component';

describe('ParsingProjectComponent', () => {
  let component: ParsingProjectComponent;
  let fixture: ComponentFixture<ParsingProjectComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ParsingProjectComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ParsingProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
