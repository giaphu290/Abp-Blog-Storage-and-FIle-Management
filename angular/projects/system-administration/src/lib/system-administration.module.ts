import { NgModule, NgModuleFactory, ModuleWithProviders } from '@angular/core';
import { CoreModule, LazyModuleFactory } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { SystemAdministrationComponent } from './components/system-administration.component';
import { SystemAdministrationRoutingModule } from './system-administration-routing.module';

@NgModule({
  declarations: [SystemAdministrationComponent],
  imports: [CoreModule, ThemeSharedModule, SystemAdministrationRoutingModule],
  exports: [SystemAdministrationComponent],
})
export class SystemAdministrationModule {
  static forChild(): ModuleWithProviders<SystemAdministrationModule> {
    return {
      ngModule: SystemAdministrationModule,
      providers: [],
    };
  }

  static forLazy(): NgModuleFactory<SystemAdministrationModule> {
    return new LazyModuleFactory(SystemAdministrationModule.forChild());
  }
}
