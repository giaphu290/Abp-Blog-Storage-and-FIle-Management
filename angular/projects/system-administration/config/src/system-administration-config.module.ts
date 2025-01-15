import { ModuleWithProviders, NgModule } from '@angular/core';
import { SYSTEM_ADMINISTRATION_ROUTE_PROVIDERS } from './providers/route.provider';

@NgModule()
export class SystemAdministrationConfigModule {
  static forRoot(): ModuleWithProviders<SystemAdministrationConfigModule> {
    return {
      ngModule: SystemAdministrationConfigModule,
      providers: [SYSTEM_ADMINISTRATION_ROUTE_PROVIDERS],
    };
  }
}
