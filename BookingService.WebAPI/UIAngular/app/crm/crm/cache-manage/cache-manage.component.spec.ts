import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CacheManageComponent } from './cache-manage.component';

describe('CacheManageComponent', () => {
  let component: CacheManageComponent;
  let fixture: ComponentFixture<CacheManageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CacheManageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CacheManageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
