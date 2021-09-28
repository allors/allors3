import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { format, formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Action, DeleteService, EditService, Filter, MediaService, NavigationService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { CommunicationEvent, displayName } from '@allors/workspace/domain/default';

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
  templateUrl: './communicationevent-list.component.html',
  providers: [SessionService],
})
export class CommunicationEventListComponent extends TestScope implements OnInit, OnDestroy {
  public title = 'Communications';

  table: Table<Row>;

  delete: Action;
  edit: Action;

  private subscription: Subscription;
  filter: Filter;

  constructor(
    @Self() public allors: SessionService,

    public refreshService: RefreshService,
    public deleteService: DeleteService,
    public editService: EditService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    titleService: Title
  ) {
    super();

    titleService.setTitle(this.title);

    this.delete = deleteService.delete(allors.client, allors.session);
    this.edit = editService.edit();

    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'type' }, { name: 'state' }, { name: 'subject', sort: true }, { name: 'involved' }, { name: 'started' }, { name: 'ended' }, { name: 'lastModifiedDate', sort: true }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = m.CommunicationEvent.filter = m.CommunicationEvent.filter ?? new Filter(m.CommunicationEvent.filterDefinition);

    this.subscription = combineLatest([this.refreshService.refresh$, this.filter.fields$, this.table.sort$, this.table.pager$])
      .pipe(
        scan(([previousRefresh, previousFilterFields], [refresh, filterFields, sort, pageEvent]) => {
          pageEvent =
            previousRefresh !== refresh || filterFields !== previousFilterFields
              ? {
                  ...pageEvent,
                  pageIndex: 0,
                }
              : pageEvent;

          if (pageEvent.pageIndex === 0) {
            this.table.pageIndex = 0;
          }

          return [refresh, filterFields, sort, pageEvent];
        }),
        switchMap(([, filterFields, sort, pageEvent]) => {
          const pulls = [
            pull.CommunicationEvent({
              predicate: this.filter.definition.predicate,
              sorting: sort ? m.CommunicationEvent.sorter.create(sort) : null,
              include: {
                CommunicationEventState: x,
                InvolvedParties: x,
              },
              arguments: this.filter.parameters(filterFields),
              skip: pageEvent.pageIndex * pageEvent.pageSize,
              take: pageEvent.pageSize,
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.session.reset();
        const communicationEvents = loaded.collection<CommunicationEvent>(m.CommunicationEvent);
        this.table.total = loaded.value('CommunicationEvents_total') as number;
        this.table.data = communicationEvents.map((v) => {
          return {
            object: v,
            type: v.strategy.cls.singularName,
            state: v.CommunicationEventState && v.CommunicationEventState.Name,
            subject: v.Subject,
            involved: v.InvolvedParties.map((w) => displayName(w)).join(', '),
            started: v.ActualStart && format(new Date(v.ActualStart), 'dd-MM-yyyy'),
            ended: v.ActualEnd && format(new Date(v.ActualEnd), 'dd-MM-yyyy'),
            lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
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
