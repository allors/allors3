import { Component, Self, OnInit, HostBinding } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import {
  PanelService,
  AllorsPanelDetailComponent,
} from '@allors/base/workspace/angular/application';
import { angularIcon } from '../../../meta/angular-icon';
import { RoleType } from '@allors/system/workspace/meta';

@Component({
  selector: 'a-mat-dyn-detail-panel',
  templateUrl: './dynamic-detail-panel.component.html',
  providers: [PanelService],
})
export class AllorsMaterialDynamicDetailPanelComponent
  extends AllorsPanelDetailComponent<IObject>
  implements OnInit
{
  anchor: RoleType;
  target: RoleType;
  @HostBinding('class.expanded-panel')
  get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  constructor(@Self() panel: PanelService) {
    super(panel);
  }

  ngOnInit() {
    this.panel.name = this.target.pluralName;
    this.panel.title = this.target.pluralName;
    this.panel.icon = angularIcon(this.panel.manager.objectType);
    this.panel.expandable = true;
  }
}
