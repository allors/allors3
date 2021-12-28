import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { formatDistance } from 'date-fns';

import { Notification } from '@allors/workspace/domain/default';
import { Action, Filter, FilterDefinition, MediaService, MethodService, NavigationService, ObjectService, RefreshService, Table, TableRow, UserId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { M } from '@allors/workspace/meta/default';
import { And } from '@allors/workspace/domain/system';

interface Row extends TableRow {
  object: Notification;
  title: string;
  description: string;
  dateCreated: string;
}

@Component({
  templateUrl: './notification-list.component.html',
  providers: [ContextService],
})
export class NotificationListComponent implements OnInit, OnDestroy {
  public title = 'Notifications';

  table: Table<Row>;

  confirm: Action;

  private subscription: Subscription;

  filter: Filter;

  m: M;

  constructor(
    @Self() public allors: ContextService,
    public factoryService: ObjectService,
    public refreshService: RefreshService,
    public methodService: MethodService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private userId: UserId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    const m = this.m;

    titleService.setTitle(this.title);

    this.confirm = methodService.create(allors.context, m.Notification.Confirm, { name: 'Confirm' });

    this.table = new Table({
      selection: true,
      columns: ['title', 'description', 'dateCreated'],
      actions: [this.confirm],
      pageSize: 50,
      initialSort: 'dateCreated',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const predicate: And = { kind: 'And', operands: [{ kind: 'Like', roleType: m.Notification.Confirmed, parameter: 'confirmed' }] };

    const filterDefinition = new FilterDefinition(predicate);
    this.filter = new Filter(filterDefinition);

    this.subscription = combineLatest(this.refreshService.refresh$, this.table.sort$, this.table.pager$)
      .pipe(
        scan(([previousRefresh], [refresh, sort, pageEvent]) => {
          pageEvent =
            previousRefresh !== refresh
              ? {
                  ...pageEvent,
                  pageIndex: 0,
                }
              : pageEvent;

          if (pageEvent.pageIndex === 0) {
            this.table.pageIndex = 0;
          }

          return [refresh, sort, pageEvent];
        }),
        switchMap(([, ,]) => {
          const pulls = [
            pull.Person({
              objectId: this.userId.value,
              select: {
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
        const notifications = loaded.collection<Notification>(m.NotificationList.UnconfirmedNotifications);
        this.table.total = notifications?.length ?? 0;
        this.table.data = notifications?.map((v) => {
          return {
            object: v,
            title: v.Title,
            description: v.Description,
            dateCreated: formatDistance(new Date(v.DateCreated), new Date()),
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
