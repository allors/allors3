import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  RequestForQuote,
  RequestItem,
  Request,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  MethodService,
  NavigationService,
  ObjectData,
  ObjectService,
  OldPanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: RequestItem;
  item: string;
  state: string;
  quantity: number;
  lastModifiedDate: string;
}

@Component({
  selector: 'requestitem-overview-panel',
  templateUrl: './requestitem-overview-panel.component.html',
  providers: [ContextService, OldPanelService],
})
export class RequestItemOverviewPanelComponent {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  request: RequestForQuote;
  requestItems: RequestItem[];
  table: Table<Row>;
  requestItem: RequestItem;

  delete: Action;
  edit: Action;
  cancel: Action;
  hold: Action;
  submit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
      associationRoleType: this.m.Request.RequestItems,
    };
  }

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public objectService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public deleteService: DeleteService,
    public editService: EditService,
    public snackBar: MatSnackBar
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'requestitem';
    panel.title = 'Request Items';
    panel.icon = 'contacts';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = this.editService.edit();
    this.cancel = methodService.create(
      allors.context,
      this.m.RequestItem.Cancel,
      { name: 'Cancel' }
    );
    this.hold = methodService.create(allors.context, this.m.RequestItem.Hold, {
      name: 'Hold',
    });
    this.submit = methodService.create(
      allors.context,
      this.m.RequestItem.Submit,
      { name: 'Submit' }
    );

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'item', sort },
        { name: 'state', sort },
        { name: 'quantity', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.edit, this.delete, this.cancel, this.hold, this.submit],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.RequestItem.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.RequestForQuote({
          name: pullName,
          objectId: id,
          select: {
            RequestItems: {
              include: {
                RequestItemState: x,
                Product: x,
                SerialisedItem: x,
              },
            },
          },
        }),
        pull.Request({
          name: 'Request',
          objectId: id,
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.requestItems = loaded.collection<RequestItem>(pullName);
      this.request = loaded.object<RequestForQuote>(this.m.Request);
      this.table.total =
        (loaded.value(`${pullName}_total`) as number) ??
        this.requestItems?.length ??
        0;
      this.table.data = this.requestItems?.map((v) => {
        return {
          object: v,
          item:
            (v.Product && v.Product.Name) ||
            (v.SerialisedItem && v.SerialisedItem.Name) ||
            '',
          state: v.RequestItemState ? v.RequestItemState.Name : '',
          quantity: v.Quantity,
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
