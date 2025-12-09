import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  ErrorService,
  InvokeService,
  MediaService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  Person,
  User,
} from '@allors/default/workspace/domain';

@Component({
  selector: 'person-summary-panel',
  templateUrl: './person-summary-panel.component.html',
})
export class PersonSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  person: Person;
  organisation: Organisation;
  contactKindsText: string;
  organisationContactRelationships: OrganisationContactRelationship[];
  user: User;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private mediaService: MediaService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService,
    public navigation: NavigationService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.Person({
        name: prefix,
        objectId: id,
        include: {
          Locale: {},
          LastModifiedBy: {},
          Salutation: {},
          Picture: {},
          CurrentPartyContactMechanisms: {
            ContactPurposes: {},
            ContactMechanism: {
              PostalAddress_Country: {},
            },
          },
          GeneralCorrespondence: {},
        },
      }),
      p.Person({
        name: `${prefix}2`,
        objectId: id,
        select: {
          OrganisationContactRelationshipsWhereContact: {
            include: {
              Organisation: {},
              ContactKinds: {},
            },
          },
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.person = loaded.object<Person>(prefix);
    this.user = loaded.object<User>(prefix);

    this.organisationContactRelationships =
      loaded.collection<OrganisationContactRelationship>(`${prefix}2`);

    if (this.organisationContactRelationships?.length > 0) {
      const organisationContactRelationship =
        this.organisationContactRelationships[0];
      this.organisation =
        organisationContactRelationship.Organisation as Organisation;

      if (organisationContactRelationship.ContactKinds.length > 0) {
        this.contactKindsText =
          organisationContactRelationship.ContactKinds?.map(
            (v: OrganisationContactKind) => v.Description
          )?.reduce((acc: string, cur: string) => acc + ', ' + cur);
      }
    }
  }

  public ResetPassword(): void {
    this.invokeService.invoke(this.user.ResetPassword).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Password reset mail send to user.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  get src(): string {
    const media = this.person.Picture;
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
