import { Component } from '@angular/core';
import { Subscription } from 'rxjs';

import { M } from '@allors/default/workspace/meta';
import { Task } from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'taskassignment-link',
  templateUrl: './taskassignment-link.component.html',
})
export class TaskAssignmentLinkComponent implements SharedPullHandler {
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
    public sharedPullService: SharedPullService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private userId: UserId
  ) {
    this.m = this.workspaceService.metaPopulation as M;
    this.sharedPullService.register(this);
  }

  onPreSharedPull(pulls: Pull[], prefix: string): void {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Task({
        name: prefix,
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'Contains',
              propertyType: this.m.Task.Participants,
              objectId: this.userId.value,
            },
          ],
        },
        include: {
          Participants: {},
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix: string): void {
    this.tasks = pullResult.collection<Task>(prefix) ?? [];
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
