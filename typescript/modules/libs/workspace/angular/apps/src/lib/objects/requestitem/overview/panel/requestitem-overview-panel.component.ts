import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { RequestForQuote, RequestItem, Request } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: RequestItem;
  item: string;
  state: string;
  quantity: number;
  lastModifiedDate: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'requestitem-overview-panel',
  templateUrl: './requestitem-overview-panel.component.html',
  providers: [SessionService, PanelService],
})
export class RequestItemOverviewPanelComponent extends TestScope {
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
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public deleteService: DeleteService,
    public editService: EditService,
    public snackBar: MatSnackBar
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'requestitem';
    panel.title = 'Request Items';
    panel.icon = 'contacts';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.client, panel.manager.session);
    this.edit = this.editService.edit();
    this.cancel = methodService.create(allors.client, allors.session, this.m.RequestItem.Cancel, { name: 'Cancel' });
    this.hold = methodService.create(allors.client, allors.session, this.m.RequestItem.Hold, { name: 'Hold' });
    this.submit = methodService.create(allors.client, allors.session, this.m.RequestItem.Submit, { name: 'Submit' });

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
      this.table.total = (loaded.value(`${pullName}_total`) as number) ?? this.requestItems.length;
      this.table.data = this.requestItems.map((v) => {
        return {
          object: v,
          item: (v.Product && v.Product.Name) || (v.SerialisedItem && v.SerialisedItem.Name) || '',
          state: v.RequestItemState ? v.RequestItemState.Name : '',
          quantity: v.Quantity,
          lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
        } as Row;
      });
    };
  }
}
