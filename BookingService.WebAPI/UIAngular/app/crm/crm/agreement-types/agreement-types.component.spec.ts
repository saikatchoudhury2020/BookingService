import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AgreementTypesComponent } from './agreement-types.component';

describe('AgreementTypesComponent', () => {
  let component: AgreementTypesComponent;
  let fixture: ComponentFixture<AgreementTypesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AgreementTypesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AgreementTypesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
