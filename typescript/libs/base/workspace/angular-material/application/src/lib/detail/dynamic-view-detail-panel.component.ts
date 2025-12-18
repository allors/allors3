import { Component } from '@angular/core';
import {
  AllorsViewDetailPanelComponent,
  ScopedService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import {
  MetaService,
  RefreshService,
  SharedPullService,
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
    scopedService: ScopedService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    private metaService: MetaService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
  }

  onPreSharedPull(pulls: Pull[], prefix?: string): void {
    const pull: Pull = {
      objectId: this.scoped.id,
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

    this.title = `${this.metaService.singularName(
      object.strategy.cls
    )} details`;

    // TODO: Meta
    this.description = object['DisplayName'] ?? object['Name'];
  }

  toggle() {
    this.panelService.startEdit(this.panelId).subscribe();
  }
}
