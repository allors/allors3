import { Component, OnInit, Self, HostBinding } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort, WorkRequirementFulfillment } from '@allors/workspace/domain/default';
import { Action, DeleteService, NavigationService, ObjectData, PanelService, RefreshService, Table, TableRow } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { WorkRequirementFulfillmentWhereFullfilledBy } from '../../../../../../../../../meta/apps/extranet/src/lib/generated/m.g';

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
  workEffort: WorkEffort;
    @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: M;

  objects: WorkRequirementFulfillment[] = [];

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

    public deleteService: DeleteService,
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
  }

  ngOnInit() {
    this.delete = this.deleteService.delete(this.panel.manager.context);

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
      actions: [this.delete],
      autoSort: true,
      autoFilter: true,
    });

    const workEffortPullName = `${this.panel.name}_${this.m.WorkRequirementFulfillment.tag}_workEffort`;
    const fixedAssetPullName = `${this.panel.name}_${this.m.WorkRequirementFulfillment.tag}_fixedAsset`;

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
            WorkRequirementsWhereFixedAsset: {
              WorkRequirementFulfillmentWhereFullfilledBy: x,
            }
          },
        }),
        pull.WorkEffort({
          objectId: id,
        }),
      );
    };

    this.panel.onPulled = (loaded) => {
      this.workEffort = loaded.object<WorkEffort>(this.m.WorkEffort);

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
          workEffortNumber: v.WorkEffortNumber,
          workRequirementNumber: v.WorkRequirementNumber,
          workRequirementDescription: v.WorkRequirementDescription,
        } as Row;
      });
    };
  }
}
