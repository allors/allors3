import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { ProductQuote } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: ProductQuote;
  number: string;
  state: string;
  customer: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'productquote-overview-panel',
  templateUrl: './productquote-overview-panel.component.html',
  providers: [PanelService],
})
export class ProductQuoteOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: ProductQuote[] = [];

  delete: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: PanelService,
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

    this.panel.name = 'productquote';
    this.panel.title = 'Product Quotes';
    this.panel.icon = 'forward';
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

    const assetPullName = `${this.panel.name}_${this.m.ProductQuote.tag}_fixedasset`;
    const customerPullName = `${this.panel.name}_${this.m.ProductQuote.tag}_customer`;

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
            QuoteItemsWhereSerialisedItem: {
              QuoteWhereQuoteItem: {
                include: {
                  QuoteState: x,
                  Receiver: x,
                },
              },
            },
          },
        }),
        pull.Party({
          name: customerPullName,
          objectId: id,
          select: {
            QuotesWhereReceiver: {
              include: {
                QuoteState: x,
                Receiver: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      const fromAsset = loaded.collection<ProductQuote>(assetPullName);
      const fromParty = loaded.collection<ProductQuote>(customerPullName);

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
          number: v.QuoteNumber,
          customer: v.Receiver.DisplayName,
          state: v.QuoteState ? v.QuoteState.Name : '',
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
