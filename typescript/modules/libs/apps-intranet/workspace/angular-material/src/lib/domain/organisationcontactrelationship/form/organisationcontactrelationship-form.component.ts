import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  OrganisationContactRelationship,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './organisationcontactrelationship-form.component.html',
  providers: [ContextService],
})
export class OrganisationContactRelationshipFormComponent
  extends AllorsFormComponent<OrganisationContactRelationship>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  partyRelationship: OrganisationContactRelationship;
  title: string;
  addContact = false;

  private subscription: Subscription;
  party: Party;
  person: Person;
  organisation: Organisation;
  contactKinds: OrganisationContactKind[];
  generalContact: OrganisationContactKind;

  peopleFilter: SearchFactory;
  organisationsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};
    // this.filters = Filters;

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            pull.OrganisationContactKind({
              sorting: [
                { roleType: this.m.OrganisationContactKind.Description },
              ],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.OrganisationContactRelationship({
                objectId: this.data.id,
                include: {
                  ContactKinds: x,
                  Organisation: x,
                  Contact: x,
                  Parties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                objectId: this.data.associationId,
              })
            );
          }

          this.peopleFilter = Filters.peopleFilter(m);
          this.organisationsFilter = Filters.organisationsFilter(m);

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.contactKinds = loaded.collection<OrganisationContactKind>(
          m.OrganisationContactKind
        );
        this.generalContact = this.contactKinds?.find(
          (v) => v.UniqueId === 'eebe4d65-c452-49c9-a583-c0ffec385e98'
        );

        if (isCreate) {
          this.title = 'Add Organisation Contact';

          this.partyRelationship =
            this.allors.context.create<OrganisationContactRelationship>(
              m.OrganisationContactRelationship
            );
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.addContactKind(this.generalContact);

          this.party = loaded.object<Party>(m.Party);

          if (this.party.strategy.cls === m.Person) {
            this.person = this.party as Person;
            this.partyRelationship.Contact = this.person;
          }

          if (this.party.strategy.cls === m.Organisation) {
            this.organisation = this.party as Organisation;
            this.partyRelationship.Organisation = this.organisation;
          }
        } else {
          this.partyRelationship =
            loaded.object<OrganisationContactRelationship>(
              m.OrganisationContactRelationship
            );
          this.person = this.partyRelationship.Contact;
          this.organisation = this.partyRelationship
            .Organisation as Organisation;

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Organisation Contact';
          } else {
            this.title = 'View Organisation Contact';
          }
        }
      });
  }

  public contactAdded(contact: Person): void {
    this.partyRelationship.Contact = contact;
  }
}
