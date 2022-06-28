import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { ProjectStoreService } from '@shared/store/project/project-store.service';

import { NetworkExplorerNetworksComponent } from './network-explorer-networks.component';

describe('NetworkExplorerNetworksComponent', () => {
  let component: NetworkExplorerNetworksComponent;
  let fixture: ComponentFixture<NetworkExplorerNetworksComponent>;
  const fakeActivatedRoute = {
    snapshot: { data: {} }
  } as ActivatedRoute;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NetworkExplorerNetworksComponent],
      imports: [RouterTestingModule],
      providers: [
        HttpClient,
        HttpHandler,
        ProjectHierarchyStoreService,
        NetworkStoreService,
        ProjectStoreService,
        ActionsStateService,
        JwtHelperService,
        { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
        { provide: ActivatedRoute, useValue: fakeActivatedRoute }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkExplorerNetworksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
