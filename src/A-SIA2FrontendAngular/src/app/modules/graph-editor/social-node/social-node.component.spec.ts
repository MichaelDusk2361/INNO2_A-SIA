import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialNodeComponent } from './social-node.component';

describe('SocialNodeComponent', () => {
  let component: SocialNodeComponent;
  let fixture: ComponentFixture<SocialNodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SocialNodeComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SocialNodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
