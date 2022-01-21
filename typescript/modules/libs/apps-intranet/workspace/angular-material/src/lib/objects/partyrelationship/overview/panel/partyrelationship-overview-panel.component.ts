import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import { PartyRelationship } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: PartyRelationship;
  type: string;
  parties: string;
  from: string;
  through: string;
}

@Component({
  selector: 'partyrelationship-overview-panel',
  templateUrl: './partyrelationship-overview-panel.component.html',
  providers: [PanelService],
})
export class PartyRelationshipOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PartyRelationship[];
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
  currentPartyRelationships: PartyRelationship[];
  inactivePartyRelationships: PartyRelationship[];
  allPartyRelationships: PartyRelationship[] = [];

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
    this.panel.name = 'partyrelationship';
    this.panel.title = 'Party Relationships';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'type', sort },
        { name: 'parties', sort },
        { name: 'from', sort },
        { name: 'through', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.PartyRelationship.tag}`;
    const active = `${this.panel.name}_${this.m.PartyRelationship.tag}_active`;
    const inactive = `${this.panel.name}_${this.m.PartyRelationship.tag}_inactive`;

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
            PartyRelationshipsWhereParty: {
              include: {
                Parties: x,
              },
            },
          },
        }),
        pull.Party({
          name: active,
          objectId: id,
          select: {
            CurrentPartyRelationships: {
              include: {
                Parties: x,
              },
            },
          },
        }),
        pull.Party({
          name: inactive,
          objectId: id,
          select: {
            InactivePartyRelationships: {
              include: {
                Parties: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<PartyRelationship>(pullName);

      this.currentPartyRelationships =
        loaded.collection<PartyRelationship>(active);
      this.currentPartyRelationships = this.currentPartyRelationships?.filter(
        (v) => v.strategy.cls !== this.m.PartyFinancialRelationship
      );

      this.inactivePartyRelationships =
        loaded.collection<PartyRelationship>(inactive);
      this.inactivePartyRelationships = this.inactivePartyRelationships?.filter(
        (v) => v.strategy.cls !== this.m.PartyFinancialRelationship
      );

      this.allPartyRelationships = [];

      if (this.currentPartyRelationships != null) {
        this.allPartyRelationships = this.allPartyRelationships.concat(
          this.currentPartyRelationships
        );
      }

      if (this.inactivePartyRelationships != null) {
        this.allPartyRelationships = this.allPartyRelationships.concat(
          this.inactivePartyRelationships
        );
      }

      this.table.total = (loaded.value(`${pullName}_total`) ??
        this.currentPartyRelationships?.length ??
        0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.partyRelationships?.map((v: PartyRelationship) => {
      return {
        object: v,
        type: v.strategy.cls.singularName,
        parties: v.Parties?.map((w) => w.DisplayName).join(', '),
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through:
          v.ThroughDate != null
            ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
            : '',
      } as Row;
    });
  }

  get partyRelationships(): any {
    switch (this.collection) {
      case 'Current':
        return this.currentPartyRelationships;
      case 'Inactive':
        return this.inactivePartyRelationships;
      case 'All':
      default:
        return this.allPartyRelationships;
    }
  }
}
