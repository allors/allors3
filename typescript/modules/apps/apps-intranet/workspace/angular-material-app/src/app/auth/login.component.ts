import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { map, of, Subscription, switchMap, tap } from 'rxjs';

import {
  AuthenticationService,
  ContextService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Organisation, Person } from '@allors/default/workspace/domain';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnDestroy {
  public loginForm = this.formBuilder.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  readonly m: M;

  private subscription: Subscription;

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private allors: ContextService,
    private internalOrganisationId: InternalOrganisationId,
    public formBuilder: FormBuilder
  ) {
    this.m = allors.metaPopulation as M;
  }

  public login() {
    const { m } = this;
    const { pullBuilder: p } = m;

    const userName = this.loginForm.controls['userName'].value;
    const password = this.loginForm.controls['password'].value;

    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.subscription = this.authService
      .login$(userName, password)
      .pipe(
        switchMap((result) => {
          if (result.a) {
            const pulls = [
              p.Person({
                objectId: result.u,
                include: {
                  UserProfile: {
                    DefaultInternalOrganization: {},
                  },
                },
              }),
              p.Organisation({
                predicate: {
                  kind: 'Equals',
                  propertyType: m.Organisation.IsInternalOrganisation,
                  value: true,
                },
              }),
            ];

            return this.allors.context.pull(pulls).pipe(
              tap((loaded) => {
                const person = loaded.object<Person>(m.Person);
                const internalOrganisations = loaded.collection<Organisation>(
                  m.Organisation
                );

                const internalOrganisation =
                  person.UserProfile?.DefaultInternalOrganization ??
                  internalOrganisations[0];

                this.internalOrganisationId.value =
                  internalOrganisation?.strategy.id;
              }),
              map(() => true)
            );
          } else {
            return of(false);
          }
        })
      )
      .subscribe(
        (authenticated) => {
          if (authenticated) {
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
