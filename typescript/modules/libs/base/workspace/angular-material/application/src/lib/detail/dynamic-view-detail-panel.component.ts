import { Component, OnDestroy } from '@angular/core';
import {
  AllorsViewDetailPanelComponent,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import {
  angularDisplayName,
  OnPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  IObject,
  IPullResult,
  OnPull,
  Pull,
} from '@allors/system/workspace/domain';

@Component({
  selector: 'a-mat-dyn-view-detail-panel',
  templateUrl: './dynamic-view-detail-panel.component.html',
})
export class AllorsMaterialDynamicViewDetailPanelComponent
  extends AllorsViewDetailPanelComponent
  implements OnPull, OnDestroy
{
  title: string;
  description: string;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    workspaceService: WorkspaceService,
    private onPullService: OnPullService
  ) {
    super(overviewService, panelService, workspaceService);

    this.panelService.register(this);
    this.onPullService.register(this);
  }

  onPrePull(pulls: Pull[], prefix?: string): void {
    const pull: Pull = {
      objectId: this.overviewService.id,
      results: [
        {
          name: prefix,
        },
      ],
    };

    pulls.push(pull);
  }

  onPostPull(pullResult: IPullResult, prefix?: string): void {
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

  ngOnDestroy(): void {
    this.panelService.unregister(this);
    this.onPullService.unregister(this);
  }
}
