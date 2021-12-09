import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Locale, Organisation, UserProfile, Singleton } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SingletonId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { PersonPicture } from '../../../../../../../../meta/apps/intranet/src/lib/generated/m.g';

@Component({
  templateUrl: './userprofile-edit.component.html',
  providers: [ContextService],
})
export class UserProfileEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public userProfile: UserProfile;

  private subscription: Subscription;
  internalOrganizations: Organisation[];
  supportedLocales: Locale[];
  public confirmPassword: string;

  user: import("c:/Users/MartienvKnippenberg/source/Repos/Allors/allors3/typescript/modules/libs/workspace/domain/apps/intranet/src/index").User;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<UserProfileEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private singletonId: SingletonId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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
              predicate: { kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true },
              sorting: [{ roleType: m.Organisation.PartyName }],
            }),
          ];

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.context.reset();

        this.userProfile = loaded.object<UserProfile>(m.UserProfile);
        this.user = this.userProfile.UserWhereUserProfile;
        this.internalOrganizations = loaded.collection<Organisation>(m.Organisation);

        const singleton = loaded.object<Singleton>(m.Singleton);
        this.supportedLocales = singleton.Locales;

        this.title = 'Edit User Profile';
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.userProfile);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
