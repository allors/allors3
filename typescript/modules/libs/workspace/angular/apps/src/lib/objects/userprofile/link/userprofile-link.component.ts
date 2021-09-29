import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Person } from '@allors/workspace/domain/default';
import { Action, EditService, ObjectService, RefreshService, UserId } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'userprofile-link',
  templateUrl: './userprofile-link.component.html',
  providers: [SessionService],
})
export class UserProfileLinkComponent implements OnInit, OnDestroy {
  edit: Action;

  private subscription: Subscription;
  user: Person;
  m: M;

  constructor(
    @Self() public allors: SessionService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public editService: EditService,
    private userId: UserId
  ) {
    this.edit = editService.edit();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = this.refreshService.refresh$
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Person({
              objectId: this.userId.value,
              include: {
                UserProfile: {
                  DefaultInternalOrganization: x,
                },
              },
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        this.user = loaded.object<Person>(m.Person);
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  toUserProfile() {
    this.edit.execute(this.user.UserProfile);
  }
}
