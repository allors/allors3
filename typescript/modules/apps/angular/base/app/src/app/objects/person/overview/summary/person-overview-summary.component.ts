import { Component, Self } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation } from '@allors/workspace/domain/default';
import { MediaService, NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({

  selector: 'person-overview-summary',
  templateUrl: './person-overview-summary.component.html',
  providers: [PanelService],
})
export class PersonOverviewSummaryComponent {
  m: M;

  person: Person;
  organisation: Organisation;

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public navigation: NavigationService,
    private mediaService: MediaService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    panel.name = 'summary';

    const personPullName = `${panel.name}_${this.m.Person.tag}`;

    panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      pulls.push(
        pull.Person({
          name: personPullName,
          objectId: id,
          include: {
            Locale: x,
            Photo: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.person = loaded.object<Person>(personPullName);
    };
  }

  get src(): string {
    const media = this.person.Photo;
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
