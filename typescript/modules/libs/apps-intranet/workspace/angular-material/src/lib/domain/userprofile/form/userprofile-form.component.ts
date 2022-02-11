import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Locale,
  Organisation,
  Singleton,
  User,
  UserProfile,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './userprofile-form.component.html',
  providers: [ContextService],
})
export class UserProfileFormComponent extends AllorsFormComponent<UserProfile> {
  public m: M;
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

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Singleton({
        objectId: this.singletonId.value,
        include: {
          Locales: {},
        },
      }),
      p.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.UserProfile({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            UserWhereUserProfile: {
              Person_Picture: {},
            },
            DefaultInternalOrganization: {},
            DefaulLocale: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.user = this.object.UserWhereUserProfile;
    this.internalOrganizations = pullResult.collection<Organisation>(
      this.m.Organisation
    );

    const singleton = pullResult.object<Singleton>(this.m.Singleton);
    this.supportedLocales = singleton.Locales;
  }
}
