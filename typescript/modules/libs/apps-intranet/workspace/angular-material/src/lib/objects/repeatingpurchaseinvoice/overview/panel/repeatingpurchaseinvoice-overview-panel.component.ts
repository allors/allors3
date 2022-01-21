import { Component, Self, HostBinding } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { format } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  InternalOrganisation,
  RepeatingPurchaseInvoice,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  MethodService,
  NavigationService,
  ObjectData,
  ObjectService,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: RepeatingPurchaseInvoice;
  frequency: string;
  dayOfWeek: string;
  previousExecutionDate: string;
  nextExecutionDate: string;
  finalExecutionDate: string;
}

@Component({
  selector: 'repeatingpurchaseinvoice-overview-panel',
  templateUrl: './repeatingpurchaseinvoice-overview-panel.component.html',
  providers: [ContextService, PanelService],
})
export class RepeatingPurchaseInvoiceOverviewPanelComponent {
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
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public objectService: ObjectService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public methodService: MethodService,
    public editService: EditService,
    public deleteService: DeleteService,
    public snackBar: MatSnackBar
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'repeating purchase invoice';
    panel.title = 'Repeating Purchase Invoices';
    panel.icon = 'business';
    panel.expandable = true;

    this.delete = deleteService.delete(panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'internalOrganisation' },
        { name: 'frequency', sort },
        { name: 'dayOfWeek', sort },
        { name: 'previousExecutionDate', sort },
        { name: 'nextExecutionDate', sort },
        { name: 'finalExecutionDate', sort },
      ],
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
      this.objects = loaded.collection<RepeatingPurchaseInvoice>(pullName);

      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          internalOrganisation: v.InternalOrganisation.DisplayName,
          frequency: v.Frequency.Name,
          dayOfWeek: v.DayOfWeek && v.DayOfWeek.Name,
          previousExecutionDate:
            v.PreviousExecutionDate &&
            format(new Date(v.PreviousExecutionDate), 'dd-MM-yyyy'),
          nextExecutionDate:
            v.NextExecutionDate &&
            format(new Date(v.NextExecutionDate), 'dd-MM-yyyy'),
          finalExecutionDate:
            v.FinalExecutionDate &&
            format(new Date(v.FinalExecutionDate), 'dd-MM-yyyy'),
        } as Row;
      });
    };
  }
}
