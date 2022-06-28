import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ModalService } from '@shared/components/modal/modal.service';

import { DataPanelComponent } from './data-panel.component';
import { InspectorPanelComponent } from './inspector-panel/inspector-panel.component';
import { NetworkPanelComponent } from './network-panel/network-panel.component';
import { StructurePanelComponent } from './structure-panel/structure-panel.component';

describe('DataPanelComponent', () => {
  let component: DataPanelComponent;
  let fixture: ComponentFixture<DataPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DataPanelComponent, InspectorPanelComponent, StructurePanelComponent, NetworkPanelComponent],
      providers: [
        HttpClient,
        HttpHandler,
        ModalService,
        JwtHelperService,
        { provide: JWT_OPTIONS, useValue: JWT_OPTIONS }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DataPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
