import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { SellerService } from './seller.service';

describe('SellerService', () => {
  let service: SellerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SellerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
