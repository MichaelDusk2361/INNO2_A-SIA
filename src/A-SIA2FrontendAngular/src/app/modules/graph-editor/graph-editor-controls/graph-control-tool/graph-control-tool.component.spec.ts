import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GraphControlToolComponent } from './graph-control-tool.component';

describe('GraphControlToolComponent', () => {
  let component: GraphControlToolComponent;
  let fixture: ComponentFixture<GraphControlToolComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [GraphControlToolComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GraphControlToolComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
