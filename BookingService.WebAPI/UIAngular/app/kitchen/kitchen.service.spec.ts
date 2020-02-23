import { TestBed, inject } from '@angular/core/testing';

import { KitchenService } from './kitchen.service';

describe('KitchenService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppService]
    });
  });

  it('should be created', inject([AppService], (service: AppService) => {
    expect(service).toBeTruthy();
  }));
});
