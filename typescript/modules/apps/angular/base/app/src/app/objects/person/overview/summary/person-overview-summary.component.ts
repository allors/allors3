import { Component, Self } from '@angular/core';

import { Person, Organisation } from '@allors/workspace/domain/default';
import {
  AllorsPanelSummaryComponent,
  MediaService,
  NavigationService,
  PanelService,
  RefreshService,
} from '@allors/workspace/angular/base';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'person-overview-summary',
  templateUrl: './person-overview-summary.component.html',
  providers: [PanelService],
})
export class PersonOverviewSummaryComponent extends AllorsPanelSummaryComponent<Person> {
  organisation: Organisation;

  constructor(
    @Self() panel: PanelService,
    public navigation: NavigationService,
    private mediaService: MediaService,
    public refreshService: RefreshService,
    public snackBar: MatSnackBar
  ) {
    super(panel);

    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

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
      this.object = loaded.object<Person>(personPullName);
    };
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
