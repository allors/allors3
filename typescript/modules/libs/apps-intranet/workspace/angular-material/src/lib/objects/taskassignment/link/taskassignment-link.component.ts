import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { Task } from '@allors/default/workspace/domain';
import {
  NavigationService,
  ObjectService,
  RefreshService,
  UserId,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'taskassignment-link',
  templateUrl: './taskassignment-link.component.html',
  providers: [ContextService],
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
    @Self() public allors: ContextService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private userId: UserId
  ) {
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
            pull.Task({
              predicate: {
                kind: 'And',
                operands: [
                  {
                    kind: 'Contains',
                    propertyType: m.Task.Participants,
                    objectId: this.userId.value,
                  },
                ],
              },
              include: {
                Participants: x,
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.tasks = loaded.collection<Task>(m.Task) ?? [];
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
