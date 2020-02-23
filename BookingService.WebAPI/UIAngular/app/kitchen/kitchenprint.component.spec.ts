import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenprintComponent } from './kitchenprint.component';

describe('KitchenprintComponent', () => {
  let component: KitchenprintComponent;
  let fixture: ComponentFixture<KitchenprintComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KitchenprintComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KitchenprintComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
