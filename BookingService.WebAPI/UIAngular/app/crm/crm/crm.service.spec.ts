import { TestBed, inject } from '@angular/core/testing';

import { CrmService } from './crm.service';

describe('CrmService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CrmService]
    });
  });

  it('should be created', inject([CrmService], (service: CrmService) => {
    expect(service).toBeTruthy();
  }));
});
