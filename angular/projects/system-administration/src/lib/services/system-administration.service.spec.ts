import { TestBed } from '@angular/core/testing';

import { SystemAdministrationService } from './system-administration.service';

describe('SystemAdministrationService', () => {
  let service: SystemAdministrationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SystemAdministrationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
