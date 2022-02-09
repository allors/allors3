import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  UserProfile,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './userprofile-form.component.html',
  providers: [ContextService],
})
export class UserProfileFormComponent
  extends AllorsFormComponent<UserProfile>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public title: string;
  public subTitle: string;

  public m: M;

  public userProfile: UserProfile;

  private subscription: Subscription;
  internalOrganizations: Organisation[];
  supportedLocales: Locale[];
  public confirmPassword: string;

  user: User;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Singleton({
              objectId: this.singletonId.value,
              include: {
                Locales: x,
              },
            }),
            pull.UserProfile({
              objectId: this.data.id,
              include: {
                UserWhereUserProfile: {
                  Person_Picture: x,
                },
                DefaultInternalOrganization: x,
                DefaulLocale: x,
              },
            }),
            pull.Organisation({
              predicate: {
                kind: 'Equals',
                propertyType: m.Organisation.IsInternalOrganisation,
                value: true,
              },
              sorting: [{ roleType: m.Organisation.DisplayName }],
            }),
          ];

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.context.reset();

        this.userProfile = loaded.object<UserProfile>(m.UserProfile);
        this.user = this.userProfile.UserWhereUserProfile;
        this.internalOrganizations = loaded.collection<Organisation>(
          m.Organisation
        );

        const singleton = loaded.object<Singleton>(m.Singleton);
        this.supportedLocales = singleton.Locales;

        this.title = 'Edit User Profile';
      });
  }
}
