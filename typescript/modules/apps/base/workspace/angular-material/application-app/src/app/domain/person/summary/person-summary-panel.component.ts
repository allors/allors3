import { Component } from '@angular/core';
import { Person } from '@allors/default/workspace/domain';
import {
  MediaService,
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  ObjectService,
  NavigationService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'person-summary-panel',
  templateUrl: './person-summary-panel.component.html',
})
export class PersonSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  object: Person;

  constructor(
    objectService: ObjectService,
    panelService: PanelService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    refreshService: RefreshService,
    public navigation: NavigationService,
    private mediaService: MediaService
  ) {
    super(
      objectService,
      panelService,
      sharedPullService,
      refreshService,
      workspaceService
    );
  }

  onPreScopedPull(pulls: Pull[], scope?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Person({
        name: scope,
        objectId: this.objectInfo.id,
        include: {
          Locale: {},
          Photo: {},
        },
      })
    );
  }

  onPostScopedPull(pullResult: IPullResult, scope?: string) {
    this.object = pullResult.object<Person>(scope);
  }

  get src(): string {
    const media = this.object.Photo;
    if (media) {
      if (media.InDataUri) {
        return media.InDataUri;
      } else if (media.UniqueId) {
        return this.mediaService.url(media);
      }
    }

    return undefined;
  }
}
