import { TestBed, inject } from '@angular/core/testing';

import { ServiceAlertService } from './service-alert.service';

describe('ServiceAlertService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ServiceAlertService]
    });
  });

  it('should be created', inject([ServiceAlertService], (service: ServiceAlertService) => {
    expect(service).toBeTruthy();
  }));
});
