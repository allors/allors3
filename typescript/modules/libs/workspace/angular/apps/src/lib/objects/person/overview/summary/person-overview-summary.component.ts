import { Component, Self } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, OrganisationContactRelationship, OrganisationContactKind, Media } from '@allors/workspace/domain/default';
import { MediaService, NavigationService, PanelService, TestScope } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'person-overview-summary',
  templateUrl: './person-overview-summary.component.html',
  providers: [PanelService],
})
export class PersonOverviewSummaryComponent extends TestScope {
  m: M;

  person: Person;
  organisation: Organisation;
  contactKindsText: string;
  organisationContactRelationships: OrganisationContactRelationship[];

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,

    public navigation: NavigationService,
    private mediaService: MediaService
  ) {
    super();

    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull, treeBuilder: tree } = m;
    const x = {};

    panel.name = 'summary';

    const personPullName = `${panel.name}_${this.m.Person.tag}`;
    const organisationContactRelationshipsPullName = `${panel.name}_${this.m.OrganisationContactRelationship.tag}`;

    panel.onPull = (pulls) => {
      const id = this.panel.manager.id;

      const partyContactMechanismTree = tree.PartyContactMechanism({
        ContactPurposes: x,
        ContactMechanism: {
          PostalAddress_Country: x,
        },
      });

      pulls.push(
        pull.Person({
          name: personPullName,
          objectId: id,
          include: {
            Locale: x,
            LastModifiedBy: x,
            Salutation: x,
            Picture: x,
            PartyContactMechanisms: partyContactMechanismTree,
            CurrentPartyContactMechanisms: partyContactMechanismTree,
            InactivePartyContactMechanisms: partyContactMechanismTree,
            GeneralCorrespondence: x,
          },
        })
      );

      pulls.push(
        pull.Person({
          name: organisationContactRelationshipsPullName,
          objectId: id,
          select: {
            OrganisationContactRelationshipsWhereContact: {
              include: {
                Organisation: x,
                ContactKinds: x,
              },
            },
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.person = loaded.object<Person>(personPullName);

      this.organisationContactRelationships = loaded.collection<OrganisationContactRelationship>(organisationContactRelationshipsPullName);

      if (this.organisationContactRelationships.length > 0) {
        const organisationContactRelationship = this.organisationContactRelationships[0];
        this.organisation = organisationContactRelationship.Organisation as Organisation;

        if (organisationContactRelationship.ContactKinds.length > 0) {
          this.contactKindsText = organisationContactRelationship.ContactKinds?.map((v: OrganisationContactKind) => v.Description).reduce((acc: string, cur: string) => acc + ', ' + cur);
        }
      }
    };
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
