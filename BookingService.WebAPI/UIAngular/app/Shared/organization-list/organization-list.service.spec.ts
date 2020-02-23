import { TestBed, inject } from '@angular/core/testing';

import { OrganizationListService } from './organization-list.service';

describe('OrganizationListService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrganizationListService]
    });
  });

  it('should be created', inject([OrganizationListService], (service: OrganizationListService) => {
    expect(service).toBeTruthy();
  }));
});
