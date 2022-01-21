import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { formatDistance } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  SerialisedItem,
  WorkRequirement,
} from '@allors/default/workspace/domain';
import {
  Action,
  DeleteService,
  NavigationService,
  ObjectData,
  PanelService,
  RefreshService,
  Table,
  TableRow,
  OverviewService,
} from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: WorkRequirement;
  number: string;
  state: string;
  customer: string;
  lastModifiedDate: string;
}

@Component({
  selector: 'workrequirement-overview-panel',
  templateUrl: './workrequirement-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkRequirementOverviewPanelComponent implements OnInit {
  serialisedItem: SerialisedItem;

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkRequirement[] = [];

  delete: Action;
  table: Table<TableRow>;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    public overviewService: OverviewService,
    public deleteService: DeleteService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    const m = this.m;

    this.delete = this.deleteService.delete(this.panel.manager.context);

    this.panel.name = 'workrequirement';
    this.panel.title = 'Work Requirements';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'number', sort },
        { name: 'state', sort },
        { name: 'customer', sort },
        { name: 'lastModifiedDate', sort },
      ],
      actions: [this.overviewService.overview(), this.delete],
      defaultAction: this.overviewService.overview(),
      autoSort: true,
      autoFilter: true,
    });

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.Party({
          objectId: id,
          select: {
            RequirementsWhereOriginator: x,
          },
        }),
        pull.SerialisedItem({
          objectId: id,
          select: {
            WorkRequirementsWhereFixedAsset: x,
          },
        }),
        pull.SerialisedItem({
          objectId: id,
        })
      );
    };

    this.panel.onPulled = (loaded) => {
      this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
      const fromCustomer = loaded.collection<WorkRequirement>(
        m.Party.RequirementsWhereOriginator
      );
      const fromAsset = loaded.collection<WorkRequirement>(
        m.FixedAsset.WorkRequirementsWhereFixedAsset
      );

      if (fromCustomer != null && fromCustomer.length > 0) {
        this.objects = fromCustomer;
      }

      if (fromAsset != null && fromAsset.length > 0) {
        this.objects = fromAsset;
      }

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          number: v.RequirementNumber,
          customer: v.OriginatorName,
          state: v.RequirementStateName,
          lastModifiedDate: formatDistance(
            new Date(v.LastModifiedDate),
            new Date()
          ),
        } as Row;
      });
    };
  }
}
