import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { NetworkStoreService } from '@shared/store/network/network-store.service';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { UserService } from '@shared/backend/user.service';

import { NetworkExplorerNetworkComponent } from './network-explorer-network.component';

describe('NetworkExplorerNetworkComponent', () => {
  let component: NetworkExplorerNetworkComponent;
  let fixture: ComponentFixture<NetworkExplorerNetworkComponent>;
  const fakeActivatedRoute = {
    snapshot: { data: {} }
  } as ActivatedRoute;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NetworkExplorerNetworkComponent],
      imports: [RouterTestingModule],
      providers: [
        HttpClient,
        HttpHandler,
        ProjectHierarchyStoreService,
        NetworkStoreService,
        { provide: ActivatedRoute, useValue: fakeActivatedRoute }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkExplorerNetworkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
});
