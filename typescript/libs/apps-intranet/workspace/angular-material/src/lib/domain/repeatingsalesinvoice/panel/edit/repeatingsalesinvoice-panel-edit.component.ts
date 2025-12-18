import { format } from 'date-fns';
import { Component, HostBinding, OnInit } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  IObject,
  IPullResult,
  Pull,
  Initializer,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  AllorsCustomEditExtentPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import {
  Action,
  DisplayService,
  MetaService,
  RefreshService,
  SharedPullService,
  Table,
  TableConfig,
  TableRow,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import {
  DeleteActionService,
  IconService,
  ViewActionService,
} from '@allors/base/workspace/angular-material/application';
import {
  RepeatingSalesInvoice,
  SalesInvoice,
} from '@allors/default/workspace/domain';

interface Row extends TableRow {
  object: IObject;
  frequency: string;
  dayOfWeek: string;
  previousExecutionDate: string;
  nextExecutionDate: string;
  finalExecutionDate: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'repeatingsalesinvoice-panel-edit',
  templateUrl: './repeatingsalesinvoice-panel-edit.component.html',
})
export class RepeatingSalesInvoicePanelEditComponent
  extends AllorsCustomEditExtentPanelComponent
  implements SharedPullHandler, OnInit
{
  override readonly panelKind = 'Extent';

  override readonly panelMode = 'Edit';
  invoice: SalesInvoice;

  override get panelId() {
    return this.m?.RepeatingSalesInvoice.tag;
  }

  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  get icon() {
    return this.iconService.icon(this.m.RepeatingSalesInvoice);
  }

  get initializer(): Initializer {
    return {
      propertyType: this.m.RepeatingSalesInvoice.Source,
      id: this.scoped.id,
    };
  }

  title = 'Repeating Sales Invoices';

  m: M;

  periodSelection: PeriodSelection = PeriodSelection.Current;

  table: Table<Row>;
  delete: Action;
  view: Action;

  objects: RepeatingSalesInvoice[] = [];

  display: RoleType[];

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    metaService: MetaService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public deleteService: DeleteActionService,
    public viewService: ViewActionService,
    private iconService: IconService,
    private displayService: DisplayService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;

    panelService.register(this);
    sharedPullService.register(this);
  }

  ngOnInit() {
    this.display = this.displayService.primary(this.m.RepeatingSalesInvoice);

    this.delete = this.deleteService.delete();
    this.view = this.viewService.view();

    const tableConfig: TableConfig = {
      selection: true,
      columns: [
        { name: 'frequency' },
        { name: 'dayOfWeek' },
        { name: 'previousExecutionDate' },
        { name: 'nextExecutionDate' },
        { name: 'finalExecutionDate' },
      ],
      actions: [this.view, this.delete],
      defaultAction: this.view,
      autoSort: true,
      autoFilter: true,
    };

    this.table = new Table(tableConfig);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.panelEnabled) {
      const id = this.scoped.id;

      pulls.push(
        p.SalesInvoice({
          objectId: id,
          name: prefix,
          select: {
            RepeatingSalesInvoiceWhereSource: {
              include: {
                Frequency: {},
                DayOfWeek: {},
              },
            },
          },
        })
      );
    }
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    if (this.panelEnabled) {
      this.enabled = this.enabler ? this.enabler() : true;

      const object = pullResult.object<RepeatingSalesInvoice>(prefix);
      if (object) {
        this.objects[0] = object;
      }

      this.table.data = this.objects.map((v) => {
        const row: Row = {
          object: v,
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
        };
        return row;
      });
    }
  }

  toggle() {
    this.panelService.stopEdit().subscribe();
  }
}
