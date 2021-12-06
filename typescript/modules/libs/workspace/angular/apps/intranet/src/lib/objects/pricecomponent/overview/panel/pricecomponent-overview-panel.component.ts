import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { isBefore, isAfter, format, formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { PriceComponent } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: PriceComponent;
  type: string;
  price: string;
  from: string;
  through: string;
  lastModifiedDate: string;
}

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'pricecomponent-overview-panel',
  templateUrl: './pricecomponent-overview-panel.component.html',
  providers: [PanelService],
})
export class PriceComponentOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PriceComponent[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  priceComponentsCollection: string;

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.panel.name = 'priceComponent';
    this.panel.title = 'Price Components';
    this.panel.icon = 'business';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [{ name: 'type' }, { name: 'price', sort }, { name: 'from', sort }, { name: 'through', sort }, { name: 'lastModifiedDate' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.PriceComponent.tag}`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.Part({
          name: pullName,
          objectId: id,
          select: {
            PriceComponentsWherePart: {
              include: {
                Currency: x,
              },
            },
          },
        }),
        pull.Product({
          name: pullName,
          objectId: id,
          select: {
            PriceComponentsWhereProduct: {
              include: {
                Currency: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<PriceComponent>(pullName);

      this.table.total = (loaded.value(`${pullName}_total`) ?? this.objects?.length ?? 0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.priceComponents?.map((v) => {
      return {
        object: v,
        type: v.strategy.cls.singularName,
        price: v.Currency.IsoCode + ' ' + v.Price,
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate != null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
        lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
      } as Row;
    });
  }

  get priceComponents(): PriceComponent[] {
    switch (this.priceComponentsCollection) {
      case 'Current':
        return this.objects?.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));
      case 'Inactive':
        return this.objects?.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate && isBefore(new Date(v.ThroughDate), new Date())));
      case 'All':
      default:
        return this.objects;
    }
  }
}
