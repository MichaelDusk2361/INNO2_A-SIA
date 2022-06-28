import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { ActionsStateService } from '@shared/store/actions-state.service';
import { InstanceStoreService } from '@shared/store/instance/instance-store.service';
import { ProjectHierarchyStoreService } from '@shared/store/project-hierarchy/project-hierarchy-store.service';
import { ProjectStoreService } from '@shared/store/project/project-store.service';

import { NetworkExplorerProjectsComponent } from './network-explorer-projects.component';

describe('NetworkExplorerProjectsComponent', () => {
  let component: NetworkExplorerProjectsComponent;
  let fixture: ComponentFixture<NetworkExplorerProjectsComponent>;
  const fakeActivatedRoute = {
    snapshot: { data: {} }
  } as ActivatedRoute;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NetworkExplorerProjectsComponent],
      imports: [RouterTestingModule],
      providers: [
        HttpClient,
        HttpHandler,
        ProjectHierarchyStoreService,
        InstanceStoreService,
        ProjectStoreService,
        ActionsStateService,
        JwtHelperService,
        { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
        { provide: ActivatedRoute, useValue: fakeActivatedRoute }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkExplorerProjectsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
});
