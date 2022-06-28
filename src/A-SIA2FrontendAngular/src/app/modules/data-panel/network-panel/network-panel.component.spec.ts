import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ModalService } from '@shared/components/modal/modal.service';

import { NetworkPanelComponent } from './network-panel.component';

describe('NetworkPanelComponent', () => {
  let component: NetworkPanelComponent;
  let fixture: ComponentFixture<NetworkPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NetworkPanelComponent],
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
    fixture = TestBed.createComponent(NetworkPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
