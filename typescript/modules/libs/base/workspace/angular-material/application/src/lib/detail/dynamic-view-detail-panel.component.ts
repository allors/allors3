import { Component } from '@angular/core';
import {
  AllorsViewDetailPanelComponent,
  ObjectService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import {
  angularDisplayName,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'a-mat-dyn-view-detail-panel',
  templateUrl: './dynamic-view-detail-panel.component.html',
})
export class AllorsMaterialDynamicViewDetailPanelComponent extends AllorsViewDetailPanelComponent {
  title: string;
  description: string;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    workspaceService: WorkspaceService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const pull: Pull = {
      objectId: this.objectInfo.id,
      results: [
        {
          name: prefix,
        },
      ],
    };

    pulls.push(pull);
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string): void {
    const object = pullResult.object<IObject>(prefix);

    this.title = `${
      angularDisplayName(object.strategy.cls) ??
      object.strategy.cls.singularName
    } details`;

    // TODO: Meta
    this.description = object['DisplayName'] ?? object['Name'];
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
