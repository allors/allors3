import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { RequestForQuote } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  NavigationService,
  ObjectData,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: RequestForQuote;
  number: string;
  state: string;
  customer: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'requestforquote-overview-panel',
  templateUrl: './requestforquote-overview-panel.component.html',
  providers: [OldPanelService],
})
export class RequestForQuoteOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: RequestForQuote[] = [];

  delete: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: OldPanelService,
    public workspaceService: WorkspaceService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.panel.name = 'requestsforquote';
    this.panel.title = 'Requests For Quote';
    this.panel.icon = 'shopping_cart';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'state', sort },
        { name: 'customer', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.overviewService.overview(), this.delete],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    const assetPullName = `${this.panel.name}_${this.m.RequestForQuote.tag}_fixedasset`;
    const customerPullName = `${this.panel.name}_${this.m.RequestForQuote.tag}_customer`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.SerialisedItem({
          name: assetPullName,
          objectId: id,
          select: {
            RequestItemsWhereSerialisedItem: {
              RequestWhereRequestItem: {
                include: {
                  RequestState: x,
                  Originator: x,
                },
              },
            },
          },
        }),
        pull.Party({
          name: customerPullName,
          objectId: id,
          select: {
            RequestsWhereOriginator: {
              include: {
                RequestState: x,
                Originator: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      const fromAsset = loaded.collection<RequestForQuote>(assetPullName);
      const fromParty = loaded.collection<RequestForQuote>(customerPullName);

      if (fromAsset != null && fromAsset.length > 0) {
        this.objects = fromAsset;
      }

      if (fromParty != null && fromParty.length > 0) {
        this.objects = fromParty;
      }

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          number: v.RequestNumber,
          customer: v.Originator && v.Originator.DisplayName,
          state: v.RequestState ? v.RequestState.Name : '',
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}