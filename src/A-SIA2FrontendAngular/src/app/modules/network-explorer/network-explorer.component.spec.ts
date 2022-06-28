import { HttpClient, HttpHandler } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { NetworkExplorerComponent } from './network-explorer.component';

describe('NetworkExplorerComponent', () => {
  let component: NetworkExplorerComponent;
  let fixture: ComponentFixture<NetworkExplorerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NetworkExplorerComponent],
      imports: [RouterTestingModule],
      providers: [HttpClient, HttpHandler]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NetworkExplorerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
});
