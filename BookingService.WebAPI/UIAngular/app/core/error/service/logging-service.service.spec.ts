import { TestBed, inject } from '@angular/core/testing';

import { LoggingServiceService } from './logging-service.service';

describe('LoggingServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LoggingServiceService]
    });
  });

  it('should be created', inject([LoggingServiceService], (service: LoggingServiceService) => {
    expect(service).toBeTruthy();
  }));
});
