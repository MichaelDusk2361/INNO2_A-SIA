import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { NetworkService } from '@shared/backend/network.service';
import { PaneService } from '../../pane.service';

import { NetworkViewComponent } from './network-view.component';

describe('NetworkViewComponent', () => {
  let component: NetworkViewComponent;
  let fixture: ComponentFixture<NetworkViewComponent>;

  beforeEach(async () => {
    const fakeActivatedRoute = {
      snapshot: { data: {} }
    } as ActivatedRoute;
    await TestBed.configureTestingModule({
      declarations: [NetworkViewComponent],

      providers: [
        { provide: ActivatedRoute, useValue: fakeActivatedRoute },
        PaneService,
        NetworkService,
        HttpClient,
        HttpHandler
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
});
