import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { of, Subscription } from 'rxjs';

import { AuthenticationService, TestScope } from '@allors/workspace/angular/base';
import { map, switchMap } from 'rxjs/operators';
import { SessionService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { IPullResult } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './login.component.html',
})
export class LoginComponent extends TestScope implements OnDestroy {
  public loginFormGhost = this.formBuilder.group({
    password: ['', Validators.required],
    userName: ['', Validators.required],
  });

  public loginForm = this.formBuilder.group({
    password: ['', Validators.required],
    userName: ['', Validators.required],
  });

  subscription: Subscription;
  m: M;

  constructor(private allors: SessionService, private authService: AuthenticationService, private router: Router, public formBuilder: FormBuilder) {
    super();
    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public login() {
    const userName = this.loginForm.controls.userName.value;
    const password = this.loginForm.controls.password.value;

    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.subscription = this.authService
      .login$(userName, password)
      .pipe(
        switchMap((result) => {
          if (result.a) {
            const m = this.m;
            const { pullBuilder: pull } = m;
            const x = {};

            const pulls = [
              pull.Organisation({
                predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              }),
              pull.Person({
                objectId: result.u,
                select: {
                  UserProfile: {
                    DefaultInternalOrganization: x,
                  },
                },
              }),
              pull.Singleton({}),
            ];

            return this.allors.client.pullReactive(this.allors.session, pulls).pipe(
              map((loaded: IPullResult) => {
                const internalOrganisations = loaded.collection<Organisation>(m.Organisation);
                const defaultInternalOrganization = loaded.object<Organisation>(m.UserProfile.DefaultInternalOrganization);

                try {
                  if (internalOrganisations && internalOrganisations.length > 0) {
                    const organisation = internalOrganisations.find((v) => v.id === this.internalOrganisationId.value);

                    if (!organisation && defaultInternalOrganization) {
                      this.internalOrganisationId.value = defaultInternalOrganization.id;
                    } else if (!organisation) {
                      this.internalOrganisationId.value = internalOrganisations[0].id;
                    }
                  }
                } catch {
                  this.internalOrganisationId.value = internalOrganisations[0].id;
                }

                const singletons = loaded.collections.Singletons as Singleton[];
                this.singletonId.value = singletons[0].id;
              })
            );
          } else {
            return of(false);
          }
        })
      )
      .subscribe(
        (result) => {
          if (result) {
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
