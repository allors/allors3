import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Person } from '@allors/workspace/domain/default';
import { Action, EditService, ObjectService, RefreshService, UserId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

@Component({
  selector: 'userprofile-link',
  templateUrl: './userprofile-link.component.html',
  providers: [ContextService],
})
export class UserProfileLinkComponent implements OnInit, OnDestroy {
  edit: Action;

  private subscription: Subscription;
  user: Person;
  m: M;

  constructor(@Self() public allors: ContextService, public factoryService: ObjectService, public refreshService: RefreshService, public editService: EditService, private userId: UserId) {
    this.edit = editService.edit();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

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
