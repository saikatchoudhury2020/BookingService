import { TestBed, inject } from '@angular/core/testing';

import { NetworkAwarePreloadStrategyService } from './network-aware-preload-strategy.service';

describe('NetworkAwarePreloadStrategyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NetworkAwarePreloadStrategyService]
    });
  });

  it('should be created', inject([NetworkAwarePreloadStrategyService], (service: NetworkAwarePreloadStrategyService) => {
    expect(service).toBeTruthy();
  }));
});
