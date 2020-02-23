import { TestBed, inject } from '@angular/core/testing';

import { OnDemandPreloadserviceService } from './on-demand-preloadservice.service';

describe('OnDemandPreloadserviceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OnDemandPreloadserviceService]
    });
  });

  it('should be created', inject([OnDemandPreloadserviceService], (service: OnDemandPreloadserviceService) => {
    expect(service).toBeTruthy();
  }));
});
