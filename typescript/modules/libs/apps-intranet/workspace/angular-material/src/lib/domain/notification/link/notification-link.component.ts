import { Component, OnInit } from '@angular/core';

import { Notification, Person } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'notification-link',
  templateUrl: './notification-link.component.html',
})
export class NotificationLinkComponent implements SharedPullHandler, OnInit {
  m: M;

  notifications: Notification[];

  get nrOfNotifications() {
    if (this.notifications) {
      const count = this.notifications.length;
      if (count < 99) {
        return count;
      } else if (count < 1000) {
        return '99+';
      } else {
        return Math.round(count / 1000) + 'k';
      }
    }

    return '?';
  }

  constructor(
    public sharedPullService: SharedPullService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private userId: UserId
  ) {
    this.m = this.workspaceService.metaPopulation as M;
    this.sharedPullService.register(this);
  }

  ngOnInit(): void {
    this.refreshService.refresh();
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
          NotificationList: {
            UnconfirmedNotifications: {},
          },
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix: string): void {
    const user = pullResult.object<Person>(prefix);
    this.notifications = user.NotificationList.UnconfirmedNotifications;
  }

  toNotifications() {
    this.navigation.list(this.m.Notification);
  }
}
