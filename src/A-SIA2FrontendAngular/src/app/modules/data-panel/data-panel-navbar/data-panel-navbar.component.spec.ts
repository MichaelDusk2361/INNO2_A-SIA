import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataPanelNavbarComponent } from './data-panel-navbar.component';

describe('DataPanelNavbarComponent', () => {
  let component: DataPanelNavbarComponent;
  let fixture: ComponentFixture<DataPanelNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DataPanelNavbarComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DataPanelNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
