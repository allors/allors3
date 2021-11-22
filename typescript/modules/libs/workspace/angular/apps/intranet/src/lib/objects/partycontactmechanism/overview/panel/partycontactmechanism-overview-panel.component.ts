import { Component, OnInit, Self, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import { PartyContactMechanism } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow, TestScope } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: PartyContactMechanism;
  purpose: string;
  contact: string;
  lastModifiedDate: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'partycontactmechanism-overview-panel',
  templateUrl: './partycontactmechanism-overview-panel.component.html',
  providers: [PanelService],
})
export class PartyContactMechanismOverviewPanelComponent extends TestScope implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: PartyContactMechanism[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  contactMechanismsCollection = 'Current';
  currentPartyContactMechanisms: PartyContactMechanism[];
  inactivePartyContactMechanisms: PartyContactMechanism[];
  allPartyContactMechanisms: PartyContactMechanism[] = [];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;
    const { pullBuilder: pull, treeBuilder: tree } = m;
    const x = {};

    this.panel.name = 'partycontactmechanism';
    this.panel.title = 'Party ContactMechanisms';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'purpose', sort },
        { name: 'contact', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.PartyContactMechanism.tag}`;
    const active = `${this.panel.name}_${this.m.PartyContactMechanism.tag}_active`;
    const inactive = `${this.panel.name}_${this.m.PartyContactMechanism.tag}_inactive`;

    this.panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      const partyContactMechanismTree = tree.PartyContactMechanism({
        ContactPurposes: x,
        ContactMechanism: {
          PostalAddress_Country: x,
        },
      });

      pulls.push(
        pull.Party({
          name: pullName,
          objectId: id,
          select: {
            PartyContactMechanisms: {
              include: partyContactMechanismTree,
            },
          },
        }),
        pull.Party({
          name: active,
          objectId: id,
          select: {
            CurrentPartyContactMechanisms: {
              include: partyContactMechanismTree,
            },
          },
        }),
        pull.Party({
          name: inactive,
          objectId: id,
          select: {
            InactivePartyContactMechanisms: {
              include: partyContactMechanismTree,
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<PartyContactMechanism>(pullName);

      this.currentPartyContactMechanisms = loaded.collection<PartyContactMechanism>(active);
      this.inactivePartyContactMechanisms = loaded.collection<PartyContactMechanism>(inactive);

      this.allPartyContactMechanisms = [];

      if (this.currentPartyContactMechanisms != null) {
        this.allPartyContactMechanisms = this.allPartyContactMechanisms.concat(this.currentPartyContactMechanisms);
      }

      if (this.inactivePartyContactMechanisms != null) {
        this.allPartyContactMechanisms = this.allPartyContactMechanisms.concat(this.inactivePartyContactMechanisms);
      }

      this.table.total = (loaded.value(`${pullName}_total`) ?? this.objects?.length ?? 0) as number;
      this.refreshTable();
    };
  }

  public refreshTable() {
    this.table.data = this.partyContactMechanisms?.map((v) => {
      return {
        object: v,
        purpose: v.ContactPurposes?.map((w) => w.Name).join(', '),
        contact: v.ContactMechanism.DisplayName,
        lastModifiedDate: formatDistance(new Date(v.LastModifiedDate), new Date()),
      } as Row;
    });
  }

  get partyContactMechanisms(): any {
    switch (this.contactMechanismsCollection) {
      case 'Current':
        return this.currentPartyContactMechanisms;
      case 'Inactive':
        return this.inactivePartyContactMechanisms;
      case 'All':
      default:
        return this.allPartyContactMechanisms;
    }
  }
}
