import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';

import { AuthenticationService } from '@allors/base/workspace/angular/foundation';

@Injectable({
  providedIn: 'root',
})
export class AuthorizationService implements CanActivate {
  constructor(
    private authenticationService: AuthenticationService,
    private router: Router
  ) {}

  public canActivate() {
    if (this.authenticationService.token) {
      return true;
    } else {
      this.router.navigate(['login']);
      return false;
    }
  }
}
