import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PersonNodeComponent } from './person-node.component';

describe('PersonNodeComponent', () => {
  let component: PersonNodeComponent;
  let fixture: ComponentFixture<PersonNodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PersonNodeComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PersonNodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
