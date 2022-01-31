import { Component } from '@angular/core';
import { Person } from '@allors/default/workspace/domain';
import {
  MediaService,
  OnPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsSummaryViewPanelComponent,
  NavigationService,
  OverviewPageService,
  PanelService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, OnPull, Pull } from '@allors/system/workspace/domain';

@Component({
  selector: 'person-summary',
  templateUrl: './person-summary.component.html',
})
export class PersonSummaryComponent
  extends AllorsSummaryViewPanelComponent
  implements OnPull
{
  object: Person;

  constructor(
    overviewService: OverviewPageService,
    panelService: PanelService,
    onPullService: OnPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    private mediaService: MediaService
  ) {
    super(overviewService, panelService, workspaceService);

    panelService.register(this);
    onPullService.register(this);
  }

  onPrePull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Person({
        name: prefix,
        objectId: this.overviewService.id,
        include: {
          Locale: {},
          Photo: {},
        },
      })
    );
  }

  onPostPull(pullResult: IPullResult, prefix?: string) {
    this.object = pullResult.object<Person>(prefix);
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
