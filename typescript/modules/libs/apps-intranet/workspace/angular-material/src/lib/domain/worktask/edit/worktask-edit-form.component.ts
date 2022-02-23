import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  InternalOrganisation,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  Priority,
  WorkEffort,
  WorkEffortPurpose,
  WorkEffortState,
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
  selector: 'worktask-edit-form',
  templateUrl: './worktask-edit-form.component.html',
  providers: [ContextService],
})
export class WorkTaskEditFormComponent extends AllorsFormComponent<WorkTask> {
  readonly m: M;
  party: Party;
  workEffortStates: WorkEffortState[];
  priorities: Priority[];
  workEffortPurposes: WorkEffortPurpose[];
  employees: Person[];
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  addContactPerson = false;
  addContactMechanism: boolean;
  workEfforts: WorkEffort[];
  customersFilter: SearchFactory;
  subContractorsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      this.internalOrganisationId.value
    );
    this.subContractorsFilter = Filters.subContractorsFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Organisation({
        objectId: this.internalOrganisationId.value,
        name: 'InternalOrganisation',
        include: {
          ActiveEmployees: {
            CurrentPartyContactMechanisms: {
              ContactMechanism: {},
            },
          },
        },
      }),
      p.WorkTask({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          WorkEffortState: {},
          FullfillContactMechanism: {},
          Priority: {},
          WorkEffortPurposes: {},
          Customer: {},
          ExecutedBy: {},
          ContactPerson: {},
          CreatedBy: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
        },
      }),
      p.Locale({
        sorting: [{ roleType: m.Locale.Name }],
      }),
      p.WorkEffortState({
        sorting: [{ roleType: m.WorkEffortState.Name }],
      }),
      p.Priority({
        predicate: {
          kind: 'Equals',
          propertyType: m.Priority.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Priority.Name }],
      }),
      p.WorkEffortPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.WorkEffortPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.WorkEffortPurpose.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    const internalOrganisation = pullResult.object<InternalOrganisation>(
      this.m.InternalOrganisation
    );
    this.employees = internalOrganisation.ActiveEmployees;

    this.workEffortStates = pullResult.collection<WorkEffortState>(
      this.m.WorkEffortState
    );
    this.priorities = pullResult.collection<Priority>(this.m.Priority);
    this.workEffortPurposes = pullResult.collection<WorkEffortPurpose>(
      this.m.WorkEffortPurpose
    );

    this.updateCustomer(this.object.Customer);
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
      pull.Party({
        object: party,
        select: {
          WorkEffortsWhereCustomer: {
            include: {
              WorkEffortState: x,
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.workEfforts = loaded.collection<WorkEffort>(
        m.Party.WorkEffortsWhereCustomer
      );
      const indexMyself = this.workEfforts.indexOf(this.object, 0);
      if (indexMyself > -1) {
        this.workEfforts.splice(indexMyself, 1);
      }

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
}
