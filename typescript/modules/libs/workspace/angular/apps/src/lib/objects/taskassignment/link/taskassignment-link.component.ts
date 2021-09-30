import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Task } from '@allors/workspace/domain/default';
import { NavigationService, ObjectService, RefreshService, UserId } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'taskassignment-link',
  templateUrl: './taskassignment-link.component.html',
  providers: [SessionService],
})
export class TaskAssignmentLinkComponent implements OnInit, OnDestroy {
  tasks: Task[];

  private subscription: Subscription;
  m: M;

  get nrOfTasks() {
    if (this.tasks) {
      const count = this.tasks.filter((v) => !v.DateClosed).length;
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
            pull.Task({
              include: {
                Participants: x,
              },
            }),
            pull.Person({
              objectId: this.userId.value,
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        const user = loaded.object<Person>(m.Person);

        const allTasks = loaded.collection<Task>(m.Task);
        this.tasks = allTasks.filter((v) => v.Participants.indexOf(user) > -1);
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  toTasks() {
    this.navigation.list(this.m.TaskAssignment);
  }
}
