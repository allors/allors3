import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { CommunicationEvent } from '@allors/default/workspace/domain';
import {
  Action,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteService,
  EditRoleService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { format, formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: CommunicationEvent;
  name: string;
  type: string;
  state: string;
  subject: string;
  involved: string;
  started: string;
  ended: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './communicationevent-list-page.component.html',
  providers: [ContextService],
})
export class CommunicationEventListPageComponent implements OnInit, OnDestroy {
  public title = 'Communications';

  table: Table<Row>;

  delete: Action;
  edit: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public refreshService: RefreshService,
    public deleteService: DeleteService,
    public editRoleService: EditRoleService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.edit = editRoleService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'type' },
        { name: 'state' },
        { name: 'subject', sort: true },
        { name: 'involved' },
        { name: 'started' },
        { name: 'ended' },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.CommunicationEvent);

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
          ([, filterFields, sort, pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.CommunicationEvent({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService
                      .sorter(m.CommunicationEvent)
                      ?.create(sort)
                  : null,
                include: {
                  CommunicationEventState: x,
                  InvolvedParties: x,
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
        const communicationEvents = loaded.collection<CommunicationEvent>(
          m.CommunicationEvent
        );
        this.table.total = (loaded.value('CommunicationEvents_total') ??
          0) as number;
        this.table.data = communicationEvents?.map((v) => {
          return {
            object: v,
            type: v.strategy.cls.singularName,
            state: v.CommunicationEventState && v.CommunicationEventState.Name,
            subject: v.Subject,
            involved: v.InvolvedParties?.map((w) => w.DisplayName).join(', '),
            started:
              v.ActualStart && format(new Date(v.ActualStart), 'dd-MM-yyyy'),
            ended: v.ActualEnd && format(new Date(v.ActualEnd), 'dd-MM-yyyy'),
            lastModifiedDate: formatDistance(
              new Date(v.LastModifiedDate),
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
