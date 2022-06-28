import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphControlZoomComponent } from './graph-control-zoom.component';

describe('GraphControlZoomComponent', () => {
  let component: GraphControlZoomComponent;
  let fixture: ComponentFixture<GraphControlZoomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GraphControlZoomComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphControlZoomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
