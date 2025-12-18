import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'allors-root',
  templateUrl: './app.component.html',
})
export class AppComponent {
  title = 'angular-apps-app';

  constructor(public ngZone: NgZone, public router: Router) {}
}
