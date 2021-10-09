import { Component, OnDestroy, Self } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { of, Subscription } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { ContextService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';
import { Organisation, Singleton } from '@allors/workspace/domain/default';
import { IPullResult } from '@allors/workspace/domain/system';
import { AuthenticationService, SingletonId, TestScope } from '@allors/workspace/angular/base';
import { InternalOrganisationId } from '@allors/workspace/angular/apps';

@Component({
  templateUrl: './login.component.html',
  providers: [ContextService],
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

  constructor(
    @Self() private allors: ContextService,
    private authService: AuthenticationService,
    private singletonId: SingletonId,
    private internalOrganisationId: InternalOrganisationId,
    private router: Router,
    public formBuilder: FormBuilder
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
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
              pull.Singleton({}),
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
            ];

            return this.allors.context.pull(pulls).pipe(
              map((loaded: IPullResult) => {
                const internalOrganisations = loaded.collection<Organisation>(m.Organisation);
                const defaultInternalOrganization = loaded.object<Organisation>(m.UserProfile.DefaultInternalOrganization);

                try {
                  if (internalOrganisations.length > 0) {
                    const organisation = internalOrganisations.find((v) => v.id === this.internalOrganisationId.value);

                    if (!organisation) {
                      if (defaultInternalOrganization) {
                        this.internalOrganisationId.value = defaultInternalOrganization.id;
                      } else {
                        this.internalOrganisationId.value = internalOrganisations[0].id;
                      }
                    }
                  }
                } catch {
                  this.internalOrganisationId.value = internalOrganisations[0].id;
                }

                const singletons = loaded.collection<Singleton>(m.Singleton);
                this.singletonId.value = singletons[0].id;

                return { init: true };
              })
            );
          } else {
            return of({ init: false });
          }
        })
      )
      .subscribe(
        ({ init }) => {
          if (init) {
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
