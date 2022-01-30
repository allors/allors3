import { Component, Self } from '@angular/core';

import { Person, Organisation } from '@allors/default/workspace/domain';
import {
  MediaService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  AllorsSummaryPanelComponent,
  NavigationService,
  OldPanelService,
} from '@allors/base/workspace/angular/application';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'person-summary',
  templateUrl: './person-summary.component.html',
  providers: [OldPanelService],
})
export class PersonSummaryComponent extends AllorsSummaryPanelComponent<Person> {
  organisation: Organisation;

  constructor(
    @Self() panel: OldPanelService,
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
