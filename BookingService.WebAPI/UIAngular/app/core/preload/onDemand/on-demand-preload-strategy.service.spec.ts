import { TestBed, inject } from '@angular/core/testing';

import { OnDemandPreloadStrategyService } from './on-demand-preload-strategy.service';

describe('OnDemandPreloadStrategyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OnDemandPreloadStrategyService]
    });
  });

  it('should be created', inject([OnDemandPreloadStrategyService], (service: OnDemandPreloadStrategyService) => {
    expect(service).toBeTruthy();
  }));
});
