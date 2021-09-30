import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Notification, Person } from '@allors/workspace/domain/default';
import { NavigationService, ObjectService, RefreshService, UserId } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'notification-link',
  templateUrl: './notification-link.component.html',
  providers: [SessionService],
})
export class NotificationLinkComponent implements OnInit, OnDestroy {
  notifications: Notification[];

  private subscription: Subscription;
  m: any;

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
    @Self() public allors: SessionService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private userId: UserId
  ) {
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
              object: this.userId.value,
              include: {
                NotificationList: {
                  UnconfirmedNotifications: x,
                },
              },
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        const user = loaded.object<Person>(m.Person);
        this.notifications = user.NotificationList.UnconfirmedNotifications;
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  toNotifications() {
    this.navigation.list(this.m.Notification);
  }
}
