import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  InternalOrganisation,
  Locale,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  SerialisedItem,
  WorkEffortFixedAssetAssignment,
  WorkTask,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './worktask-create-form.component.html',
  providers: [ContextService],
})
export class WorkTaskCreateFormComponent extends AllorsFormComponent<WorkTask> {
  readonly m: M;

  internalOrganisation: InternalOrganisation;
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  addContactPerson = false;
  addContactMechanism: boolean;

  locales: Locale[];
  organisationsFilter: SearchFactory;
  subContractorsFilter: SearchFactory;
  workEffortFixedAssetAssignment: WorkEffortFixedAssetAssignment;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationsFilter = Filters.organisationsFilter(this.m);
    this.subContractorsFilter = Filters.subContractorsFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Locale({
        sorting: [{ roleType: m.Locale.Name }],
      })
    );

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.SerialisedItem({
          objectId: initializer.id,
        }),
        p.Party({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.locales = pullResult.collection<Locale>(this.m.Locale);

    const fromSerialiseditem = pullResult.object<SerialisedItem>(
      this.m.SerialisedItem
    );
    const fromCustomer = pullResult.object<Party>(this.m.Party);

    this.object.TakenBy = this.internalOrganisation as Organisation;
    this.object.Customer = fromCustomer;

    if (fromSerialiseditem != null) {
      this.workEffortFixedAssetAssignment =
        this.allors.context.create<WorkEffortFixedAssetAssignment>(
          this.m.WorkEffortFixedAssetAssignment
        );
      this.workEffortFixedAssetAssignment.Assignment = this.object;
      this.workEffortFixedAssetAssignment.FixedAsset = fromSerialiseditem;
    }
  }

  public customerSelected(customer: IObject) {
    this.updateCustomer(customer as Party);
  }

  private updateCustomer(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: party,
        select: {
          PartyContactMechanismsWhereParty: x,
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );

      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  public contactPersonAdded(contact: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .Customer as Organisation;
    organisationContactRelationship.Contact = contact;

    this.contacts.push(contact);
    this.object.ContactPerson = contact;
  }

  public contactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.Customer;
    this.object.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }
}
