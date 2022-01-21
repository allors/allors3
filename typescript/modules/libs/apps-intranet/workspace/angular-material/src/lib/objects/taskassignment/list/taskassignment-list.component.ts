import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { TaskAssignment } from '@allors/default/workspace/domain';
import {
  Action,
  EditService,
  Filter,
  FilterDefinition,
  FilterField,
  MediaService,
  NavigationService,
  ObjectService,
  RefreshService,
  Table,
  TableRow,
  UserId,
} from '@allors/workspace/angular/base';
import {
  ContextService,
  WorkspaceService,
} from '@allors/workspace/angular/core';
import { And } from '@allors/system/workspace/domain';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';

interface Row extends TableRow {
  object: TaskAssignment;
  title: string;
  dateCreated: string;
}

@Component({
  templateUrl: './taskassignment-list.component.html',
  providers: [ContextService],
})
export class TaskAssignmentListComponent implements OnInit, OnDestroy {
  public title = 'Tasks';

  table: Table<Row>;

  edit: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public workspaceService: WorkspaceService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public editService: EditService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private userId: UserId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    this.edit = editService.edit(this.m.TaskAssignment.Task);
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
                // sorting: sorter.create(sort),
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
        this.table.total = (loaded.value('TaskAssignments_total') ??
          0) as number;
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
