import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SystemAdministrationComponent } from './components/system-administration.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: SystemAdministrationComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SystemAdministrationRoutingModule {}
