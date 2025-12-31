import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';

import { AuthenticationService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'allors-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'Angular Core';

  constructor(
    public ngZone: NgZone,
    public router: Router,
    public authService: AuthenticationService
  ) {}

  logout() {
    this.authService.logout();
  }
}
