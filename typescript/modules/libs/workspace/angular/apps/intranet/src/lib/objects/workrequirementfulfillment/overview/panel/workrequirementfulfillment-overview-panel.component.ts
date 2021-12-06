import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { WorkRequirementFulfillment } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

interface Row extends TableRow {
  object: WorkRequirementFulfillment;
  workEffortNumber: string;
  workRequirementNumber: string;
  workRequirementDescription: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workrequirementfulfillment-overview-panel',
  templateUrl: './workrequirementfulfillment-overview-panel.component.html',
  providers: [PanelService],
})
export class WorkRequirementFulfillmentOverviewPanelComponent implements OnInit {
    @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkRequirementFulfillment[] = [];

  delete: Action;
  edit: Action;
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

    public deleteService: DeleteService,
    public editService: EditService
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    this.panel.name = 'workrequirementfulfillment';
    this.panel.title = 'Work Requirement Fulfillment';
    this.panel.icon = 'work';
    this.panel.expandable = true;

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'workEffortNumber', sort },
        { name: 'workRequirementNumber', sort },
        { name: 'workRequirementDescription', sort },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const workEffortPullName = `${this.panel.name}_${this.m.WorkEffortFixedAssetAssignment.tag}_workEffort`;
    const fixedAssetPullName = `${this.panel.name}_${this.m.WorkEffortFixedAssetAssignment.tag}_fixedAsset`;

    this.panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const id = this.panel.manager.id;

      pulls.push(
        pull.WorkEffort({
          name: workEffortPullName,
          objectId: id,
          select: {
            WorkRequirementFulfillmentsWhereFullfillmentOf: x,
          },
        }),
        pull.FixedAsset({
          name: fixedAssetPullName,
          objectId: id,
          select: {
            WorkRequirementFulfillmentsWhereFixedAsset: x,
          },
        }),
      );
    };

    this.panel.onPulled = (loaded) => {
      const fromWorkEffort = loaded.collection<WorkRequirementFulfillment>(workEffortPullName);
      const fromFixedAsset = loaded.collection<WorkRequirementFulfillment>(fixedAssetPullName);

      if (fromWorkEffort != null && fromWorkEffort.length > 0) {
        this.objects = fromWorkEffort;
      }

      if (fromFixedAsset != null && fromFixedAsset.length > 0) {
        this.objects = fromFixedAsset;
      }

      this.table.total = this.objects?.length ?? 0;
      this.table.data = this.objects?.map((v) => {
        return {
          object: v,
          workEffortNumber: v.WorkEffortName,
          workRequirementNumber: v.WorkEffortName,
          workRequirementDescription: v.WorkRequirementDescription,
        } as Row;
      });
    };
  }
}
