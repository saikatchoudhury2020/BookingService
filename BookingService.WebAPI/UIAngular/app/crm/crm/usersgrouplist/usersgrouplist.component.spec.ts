import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UsersgrouplistComponent } from './usersgrouplist.component';

describe('UsersgrouplistComponent', () => {
  let component: UsersgrouplistComponent;
  let fixture: ComponentFixture<UsersgrouplistComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UsersgrouplistComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UsersgrouplistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
