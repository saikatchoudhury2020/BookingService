import { TestBed, inject } from '@angular/core/testing';

import { OptInPreloadStrategyService } from './opt-in-preload-strategy.service';

describe('OptInPreloadStrategyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OptInPreloadStrategyService]
    });
  });

  it('should be created', inject([OptInPreloadStrategyService], (service: OptInPreloadStrategyService) => {
    expect(service).toBeTruthy();
  }));
});
