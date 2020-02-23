import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceAlertComponent } from './service-alert.component';

describe('ServiceAlertComponent', () => {
  let component: ServiceAlertComponent;
  let fixture: ComponentFixture<ServiceAlertComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ServiceAlertComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServiceAlertComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
