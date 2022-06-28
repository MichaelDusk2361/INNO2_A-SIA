import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaneIfDirective } from './pane-if.directive';
import { PaneComponent } from './pane/pane.component';

describe('PaneIfDirective', () => {
  let fixture: ComponentFixture<PaneComponent>;
  beforeEach(() => {
    fixture = TestBed.configureTestingModule({
      declarations: [PaneComponent, PaneIfDirective],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).createComponent(PaneComponent);
    fixture.detectChanges(); // initial binding
  });

  it('should create an instance', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
