import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root',
})
export class SystemAdministrationService {
  apiName = 'SystemAdministration';

  constructor(private restService: RestService) {}

  sample() {
    return this.restService.request<void, any>(
      { method: 'GET', url: '/api/SystemAdministration/sample' },
      { apiName: this.apiName }
    );
  }
}
