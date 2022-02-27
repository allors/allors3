import { Component } from '@angular/core';

import { Person } from '@allors/default/workspace/domain';
import {
  Action,
  RefreshService,
  SharedPullService,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { EditActionService } from '@allors/base/workspace/angular-material/application';

@Component({
  selector: 'userprofile-link',
  templateUrl: './userprofile-link.component.html',
})
export class UserProfileLinkComponent implements SharedPullHandler {
  edit: Action;
  user: Person;
  m: M;

  constructor(
    public sharedPullService: SharedPullService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public editRoleService: EditActionService,
    private userId: UserId
  ) {
    this.edit = editRoleService.edit();

    this.m = this.workspaceService.metaPopulation as M;
    this.sharedPullService.register(this);
  }

  onPreSharedPull(pulls: Pull[], prefix: string): void {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Person({
        name: prefix,
        objectId: this.userId.value,
        include: {
          UserProfile: {
            DefaultInternalOrganization: {},
          },
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix: string): void {
    this.user = pullResult.object<Person>(prefix);
  }

  toUserProfile() {
    this.edit.execute(this.user.UserProfile);
  }
}
