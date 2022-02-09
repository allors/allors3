import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthenticationService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnDestroy {
  public loginForm = this.formBuilder.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  private subscription: Subscription;

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    public formBuilder: FormBuilder
  ) {}

  public login() {
    const userName = this.loginForm.controls['userName'].value;
    const password = this.loginForm.controls['password'].value;

    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.subscription = this.authService.login$(userName, password).subscribe(
      (result) => {
        if (result.a) {
          this.router.navigate(['/']);
        } else {
          alert('Could not log in');
        }
      },
      (error) => alert(JSON.stringify(error))
    );
  }

  public ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
