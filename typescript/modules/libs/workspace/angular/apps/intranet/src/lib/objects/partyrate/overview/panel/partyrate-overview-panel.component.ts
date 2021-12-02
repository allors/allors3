import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { format, isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { PartyRate } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: PartyRate;
  rateType: string;
  from: string;
  through: string;
  rate: string;
  frequency: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'partyrate-overview-panel',
  templateUrl: './partyrate-overview-panel.component.html',
  providers: [PanelService],
})
export class PartyRateOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PartyRate[];
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
  currentPartyRates: PartyRate[];
  inactivePartyRates: PartyRate[];
  allPartyRates: PartyRate[] = [];

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
    this.panel.name = 'partyrate';
    this.panel.title = 'Party Rates';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [{ name: 'rateType' }, { name: 'from', sort }, { name: 'through', sort }, { name: 'rate', sort }, { name: 'frequency' }],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.PartyRate.tag}`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};
      const id = this.panel.manager.id;

      pulls.push(
        pull.Party({
          name: pullName,
          objectId: id,
          select: {
            PartyRates: {
              include: {
                RateType: x,
                Frequency: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<PartyRate>(pullName);

      this.table.total = (loaded.value(`${pullName}_total`) ?? this.objects?.length ?? 0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.partyRates?.map((v) => {
      return {
        object: v,
        rateType: v.RateType.Name,
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate != null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
        rate: v.Rate,
        frequency: v.Frequency.Name,
      } as Row;
    });
  }

  get partyRates(): any {
    switch (this.collection) {
      case 'Current':
        return this.objects && this.objects?.filter((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));
      case 'Inactive':
        return this.objects && this.objects?.filter((v) => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate && isBefore(new Date(v.ThroughDate), new Date())));
      case 'All':
      default:
        return this.objects;
    }
  }
}
