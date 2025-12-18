import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { M } from '@allors/default/workspace/meta';
import { Notification } from '@allors/default/workspace/domain';
import {
  ContextService,
  FilterDefinition,
  FilterService,
  Table,
  TableRow,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import {
  Action,
  Filter,
  MediaService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  MethodActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { And } from '@allors/system/workspace/domain';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: Notification;
  title: string;
  description: string;
  dateCreated: string;
}

@Component({
  templateUrl: './notification-list-page.component.html',
  providers: [ContextService],
})
export class NotificationListPageComponent implements OnInit, OnDestroy {
  public title = 'Notifications';

  table: Table<Row>;

  confirm: Action;

  private subscription: Subscription;

  filter: Filter;

  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public methodActionService: MethodActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private userId: UserId,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    const m = this.m;

    titleService.setTitle(this.title);

    this.confirm = methodActionService.create(m.Notification.Confirm, {
      name: 'Confirm',
    });

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

    const predicate: And = {
      kind: 'And',
      operands: [
        {
          kind: 'Like',
          roleType: m.Notification.Confirmed,
          parameter: 'confirmed',
        },
      ],
    };

    const filterDefinition = new FilterDefinition(predicate);
    this.filter = new Filter(filterDefinition);

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.table.sort$,
      this.table.pager$
    )
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
        switchMap((_) => {
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
        const notifications = loaded.collection<Notification>(
          m.NotificationList.UnconfirmedNotifications
        );
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
