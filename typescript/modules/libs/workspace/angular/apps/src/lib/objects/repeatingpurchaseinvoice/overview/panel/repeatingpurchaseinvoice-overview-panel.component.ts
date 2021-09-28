import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { Organisation, InternalOrganisation, RepeatingPurchaseInvoice } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, MethodService, NavigationService, ObjectData, ObjectService, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: RepeatingPurchaseInvoice;
  frequency: string;
  dayOfWeek: string;
  previousExecutionDate: string;
  nextExecutionDate: string;
  finalExecutionDate: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'repeatingpurchaseinvoice-overview-panel',
  templateUrl: './repeatingpurchaseinvoice-overview-panel.component.html',
  providers: [SessionService, PanelService],
})
export class RepeatingPurchaseInvoiceOverviewPanelComponent extends TestScope {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  internalOrganisation: Organisation;
  objects: RepeatingPurchaseInvoice[] = [];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,

    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'repeating purchase invoice';
    panel.title = 'Repeating Purchase Invoices';
    panel.icon = 'business';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.session);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [{ name: 'internalOrganisation' }, { name: 'frequency', sort }, { name: 'dayOfWeek', sort }, { name: 'previousExecutionDate', sort }, { name: 'nextExecutionDate', sort }, { name: 'finalExecutionDate', sort }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${panel.name}_${this.m.RepeatingPurchaseInvoice.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Organisation({
          name: pullName,
          objectId: id,
          select: {
            RepeatingPurchaseInvoicesWhereSupplier: {
              include: {
                InternalOrganisation: x,
                Frequency: x,
                DayOfWeek: x,
              },
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);

      this.objects = loaded.collection<RepeatingPurchaseInvoice>(pullName);

      this.table.data = this.objects.map((v) => {
        return {
          object: v,
          internalOrganisation: v.InternalOrganisation.PartyName,
          frequency: v.Frequency.Name,
          dayOfWeek: v.DayOfWeek && v.DayOfWeek.Name,
          previousExecutionDate: v.PreviousExecutionDate && format(new Date(v.PreviousExecutionDate), 'dd-MM-yyyy'),
          nextExecutionDate: v.NextExecutionDate && format(new Date(v.NextExecutionDate), 'dd-MM-yyyy'),
          finalExecutionDate: v.FinalExecutionDate && format(new Date(v.FinalExecutionDate), 'dd-MM-yyyy'),
        } as Row;
      });
    };
  }
}
