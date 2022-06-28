import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataPanelNavbarEntryComponent } from './data-panel-navbar-entry.component';

describe('DataPanelNavbarEntryComponent', () => {
  let component: DataPanelNavbarEntryComponent;
  let fixture: ComponentFixture<DataPanelNavbarEntryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DataPanelNavbarEntryComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DataPanelNavbarEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
