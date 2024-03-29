import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And } from '@allors/system/workspace/domain';
import { TaskAssignment } from '@allors/default/workspace/domain';
import {
  Action,
  Filter,
  FilterDefinition,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  EditActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: TaskAssignment;
  title: string;
  dateCreated: string;
}

@Component({
  templateUrl: './taskassignment-list-page.component.html',
  providers: [ContextService],
})
export class TaskAssignmentListPageComponent implements OnInit, OnDestroy {
  public title = 'Tasks';

  table: Table<Row>;

  edit: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public editRoleService: EditActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    private userId: UserId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    this.edit = editRoleService.edit(this.m.TaskAssignment.Task);
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: ['title', 'dateCreated'],
      actions: [this.edit],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'dateCreated',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull, treeBuilder: tree } = m;
    const x = {};

    const predicate: And = {
      kind: 'And',
      operands: [
        {
          kind: 'Equals',
          propertyType: m.TaskAssignment.User,
          value: this.userId.value,
        },
        {
          kind: 'ContainedIn',
          propertyType: m.TaskAssignment.Task,
          extent: {
            kind: 'Filter',
            objectType: m.Task,
            predicate: {
              kind: 'Like',
              roleType: m.Task.Title,
              parameter: 'title',
            },
          },
        },
      ],
    };

    const filterDefinition = new FilterDefinition(predicate);
    this.filter = new Filter(filterDefinition);

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent]
          ) => {
            pageEvent =
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? {
                    ...pageEvent,
                    pageIndex: 0,
                  }
                : pageEvent;

            if (pageEvent.pageIndex === 0) {
              this.table.pageIndex = 0;
            }

            return [refresh, filterFields, sort, pageEvent];
          }
        ),
        switchMap(
          ([, filterFields, , pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.TaskAssignment({
                predicate,
                include: {
                  Task: {
                    WorkItem: x,
                  },
                  User: x,
                },
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const taskAssignments = loaded.collection<TaskAssignment>(
          m.TaskAssignment
        );
        this.table.data = taskAssignments?.map((v) => {
          return {
            object: v,
            title: v.Task.Title,
            dateCreated: formatDistance(
              new Date(v.Task.DateCreated),
              new Date()
            ),
          } as Row;
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
