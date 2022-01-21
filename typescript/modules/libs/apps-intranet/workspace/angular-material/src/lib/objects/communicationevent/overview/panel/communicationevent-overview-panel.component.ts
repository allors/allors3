import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { CommunicationEvent } from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  EditService,
  NavigationService,
  ObjectData,
  ObjectService,
  PanelService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

interface Row extends TableRow {
  object: CommunicationEvent;
  type: string;
  description: string;
  involved: string;
  status: string;
  purpose: string;
}

@Component({
  selector: 'communicationevent-overview-panel',
  templateUrl: './communicationevent-overview-panel.component.html',
  providers: [PanelService],
})
export class CommunicationEventOverviewPanelComponent implements OnInit {
  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: CommunicationEvent[];
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
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public objectService: ObjectService,
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

    this.panel.name = 'communicationevent';
    this.panel.title = 'Communication events';
    this.panel.icon = 'message';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'type', sort },
        { name: 'description', sort },
        { name: 'involved', sort },
        { name: 'status', sort },
        { name: 'purpose', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.CommunicationEvent.tag}`;

    this.panel.onPull = (pulls) => {
      const { id } = this.panel.manager;

      pulls.push(
        pull.Party({
          name: pullName,
          objectId: id,
          select: {
            CommunicationEventsWhereInvolvedParty: {
              include: {
                InvolvedParties: x,
                CommunicationEventState: x,
                EventPurposes: x,
              },
            },
          },
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collection<CommunicationEvent>(pullName);

      this.table.total = (loaded.value(`${pullName}_total`) ??
        this.objects?.length ??
        0) as number;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          type: v.strategy.cls.singularName,
          description: v.Description,
          involved: v.InvolvedParties?.map((w) => w.DisplayName).join(', '),
          status: v.CommunicationEventState.Name,
          purpose: v.EventPurposes?.map((w) => w.Name).join(', '),
        } as Row;
      });
    };
  }
}
