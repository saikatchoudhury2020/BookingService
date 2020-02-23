import { TestBed, inject } from '@angular/core/testing';

import { ErrorServiceService } from './error-service.service';

describe('ErrorServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ErrorServiceService]
    });
  });

  it('should be created', inject([ErrorServiceService], (service: ErrorServiceService) => {
    expect(service).toBeTruthy();
  }));
});
