import { Component, OnInit } from '@angular/core';
import { SystemAdministrationService } from '../services/system-administration.service';

@Component({
  selector: 'lib-system-administration',
  template: ` <p>system-administration works!</p> `,
  styles: [],
})
export class SystemAdministrationComponent implements OnInit {
  constructor(private service: SystemAdministrationService) {}

  ngOnInit(): void {
    this.service.sample().subscribe(console.log);
  }
}
