import { TestBed } from '@angular/core/testing';

import { DetailsSidebarService } from './details-sidebar.service';

describe('DetailsSidebarService', () => {
  let service: DetailsSidebarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DetailsSidebarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
