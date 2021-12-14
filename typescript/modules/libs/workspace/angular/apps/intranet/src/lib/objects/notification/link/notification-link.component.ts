import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Notification, Person } from '@allors/workspace/domain/default';
import { NavigationService, ObjectService, RefreshService, UserId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'notification-link',
  templateUrl: './notification-link.component.html',
  providers: [ContextService],
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

  constructor(@Self() public allors: ContextService, public factoryService: ObjectService, public refreshService: RefreshService, public navigation: NavigationService, private userId: UserId) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Person({
              objectId: this.userId.value,
              include: {
                NotificationList: {
                  UnconfirmedNotifications: x,
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

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
