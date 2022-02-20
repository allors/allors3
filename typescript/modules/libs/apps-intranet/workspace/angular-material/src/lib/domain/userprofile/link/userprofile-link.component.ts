import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Person } from '@allors/default/workspace/domain';
import {
  Action,
  EditService,
  RefreshService,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { ScopedService } from '@allors/base/workspace/angular/application';
import { EditRoleService } from '@allors/base/workspace/angular-material/application';

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

  constructor(
    @Self() public allors: ContextService,
    public scopedService: ScopedService,
    public refreshService: RefreshService,
    public editRoleService: EditRoleService,
    private userId: UserId
  ) {
    this.edit = editRoleService.edit();

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
