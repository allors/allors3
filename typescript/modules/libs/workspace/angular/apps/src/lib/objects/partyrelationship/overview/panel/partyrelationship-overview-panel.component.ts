import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { format } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { PartyRelationship } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';

interface Row extends TableRow {
  object: PartyRelationship;
  type: string;
  parties: string;
  from: string;
  through: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'partyrelationship-overview-panel',
  templateUrl: './partyrelationship-overview-panel.component.html',
  providers: [PanelService],
})
export class PartyRelationshipOverviewPanelComponent extends TestScope implements OnInit {
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

    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
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
    const active = `${this.panel.name}_${this.m.PartyRelationship.name}_active`;
    const inactive = `${this.panel.name}_${this.m.PartyRelationship.name}_inactive`;

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
      this.objects = loaded.collections[pullName] as PartyRelationship[];

      this.currentPartyRelationships = loaded.collections[active] as PartyRelationship[];
      this.currentPartyRelationships = this.currentPartyRelationships.filter((v) => v.objectType.name !== this.m.PartyFinancialRelationship.name);

      this.inactivePartyRelationships = loaded.collections[inactive] as PartyRelationship[];
      this.inactivePartyRelationships = this.inactivePartyRelationships.filter((v) => v.objectType.name !== this.m.PartyFinancialRelationship.name);

      this.allPartyRelationships = [];

      if (this.currentPartyRelationships !== undefined) {
        this.allPartyRelationships = this.allPartyRelationships.concat(this.currentPartyRelationships);
      }

      if (this.inactivePartyRelationships !== undefined) {
        this.allPartyRelationships = this.allPartyRelationships.concat(this.inactivePartyRelationships);
      }

      if (this.objects) {
        this.table.total = loaded.values[`${pullName}_total`] || this.currentPartyRelationships.length;
        this.refreshTable();
      }
    };
  }

  public refreshTable() {
    this.table.data = this.partyRelationships.map((v: PartyRelationship) => {
      return {
        object: v,
        type: v.objectType.name,
        parties: v.Parties.map((w) => w.displayName).join(', '),
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate !== null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
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
