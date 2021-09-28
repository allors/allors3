import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {  SessionService, MetaService, RefreshService, UserId, NavigationService } from '@allors/angular/services/core';
import { Person, Task } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { ObjectService } from '@allors/angular/material/services/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'taskassignment-link',
  templateUrl: './taskassignment-link.component.html',
  providers: [SessionService]
})
export class TaskAssignmentLinkComponent implements OnInit, OnDestroy {

  tasks: Task[];

  private subscription: Subscription;

  get nrOfTasks() {
    if (this.tasks) {
      const count = this.tasks.filter(v => !v.DateClosed).length;
      if (count < 99) {
        return count;
      } else if (count < 1000) {
        return '99+';
      } else {
        return Math.round(count / 1000) + 'k';
      }
    }

    return "?";
  }

  constructor(
    @Self() public allors: SessionService,
    
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private userId: UserId,
    ) {
  }

  ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = this.refreshService.refresh$
      .pipe(
        switchMap(() => {

          const pulls = [
            pull.Task({
              include: {
                Participants: x
              }
            }),
            pull.Person({
              object: this.userId.value
            })];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();

        const user = loaded.object<Person>(m.Person);

        const allTasks = loaded.collection<Task>(m.Task);
        this.tasks = allTasks.filter(v => v.Participants.indexOf(user) > -1);
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  toTasks() {
    this.navigation.list(this.metaService.m.TaskAssignment);
  }
}
