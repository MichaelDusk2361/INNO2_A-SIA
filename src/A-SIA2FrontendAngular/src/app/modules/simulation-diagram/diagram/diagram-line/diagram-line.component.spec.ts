import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiagramLineComponent } from './diagram-line.component';

describe('DiagramLineComponent', () => {
  let component: DiagramLineComponent;
  let fixture: ComponentFixture<DiagramLineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DiagramLineComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DiagramLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
