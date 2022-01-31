import { Component, OnInit, HostBinding, Input } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
import {
  AllorsRelationshipViewPanelComponent,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { PeriodSelection } from '@allors/base/workspace/angular-material/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-dyn-rel-view-panel',
  templateUrl: './dynamic-relationship-view-panel.component.html',
})
export class AllorsMaterialDynamicRelationshipViewPanelComponent
  extends AllorsRelationshipViewPanelComponent
  implements OnInit
{
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return true;
    // return this.panel.isExpanded;
  }

  @Input()
  anchor: RoleType;

  @Input()
  target: RoleType;

  @Input()
  display: RoleType;

  objectType: Composite;

  hasPeriod: boolean;
  periodSelection: PeriodSelection = PeriodSelection.Current;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService
  ) {
    super(overviewService, panelService, workspaceService);
  }

  ngOnInit() {
    this.objectType = this.target.associationType.objectType as Composite;
    this.hasPeriod = this.objectType.supertypes.has(this.m.Period);
  }
}
