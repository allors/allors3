import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { format, isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { SupplierOffering } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';

interface Row extends TableRow {
  object: SupplierOffering;
  supplier: string;
  price: string;
  uom: string;
  from: string;
  through: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'supplieroffering-overview-panel',
  templateUrl: './supplieroffering-overview-panel.component.html',
  providers: [PanelService],
})
export class SupplierOfferingOverviewPanelComponent extends TestScope implements OnInit {
  currentObjects: SupplierOffering[];

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: SupplierOffering[] = [];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  collection = 'Current';

  constructor(
    @Self() public panel: PanelService,

    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.panel.name = 'supplieroffering';
    this.panel.title = 'Supplier Offerings';
    this.panel.icon = 'business';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'supplier', sort },
        { name: 'price', sort },
        { name: 'uom', sort },
        { name: 'from', sort },
        { name: 'through', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.SupplierOffering.name}`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};
      const id = this.panel.manager.id;

      pulls.push(
        pull.Part({
          name: pullName,
          objectId: id,
          select: {
            SupplierOfferingsWherePart: {
              include: {
                Currency: x,
                UnitOfMeasure: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collections[pullName] as SupplierOffering[];
      this.currentObjects = this.objects.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));

      if (this.objects) {
        this.table.total = loaded.values[`${pullName}_total`] || this.objects.length;
        this.refreshTable();
      }
    };
  }

  public refreshTable() {
    this.table.data = this.suplierOfferings.map((v) => {
      return {
        object: v,
        supplier: v.Supplier.displayName,
        price: v.Currency.IsoCode + ' ' + v.Price,
        uom: v.UnitOfMeasure.Abbreviation || v.UnitOfMeasure.Name,
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate !== null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
      } as Row;
    });
  }

  get suplierOfferings(): SupplierOffering[] {
    switch (this.collection) {
      case 'Current':
        return this.objects.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));
      case 'Inactive':
        return this.objects.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate && isBefore(new Date(v.ThroughDate), new Date())));
      case 'All':
      default:
        return this.objects;
    }
  }
}
