import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Currency,
  CustomerRelationship,
  InternalOrganisation,
  IrpfRegime,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  ProductQuote,
  RequestForQuote,
  SalesOrder,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  selector: 'productquote-edit-form',
  templateUrl: './productquote-edit-form.component.html',
  providers: [ContextService],
})
export class ProductQuoteEditFormComponent extends AllorsFormComponent<ProductQuote> {
  readonly m: M;
  salesOrder: SalesOrder;
  request: RequestForQuote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  internalOrganisation: InternalOrganisation;

  addContactPerson = false;
  addContactMechanism = false;
  addReceiver = false;

  private previousReceiver: Party;
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];

  customersFilter: SearchFactory;
  showIrpf: boolean;

  get receiverIsPerson(): boolean {
    return (
      !this.object.Receiver ||
      this.object.Receiver.strategy.cls === this.m.Person
    );
  }

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
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
      p.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
      p.ProductQuote({
        objectId: this.editRequest.objectId,
        include: {
          AssignedCurrency: {},
          DerivedCurrency: {},
          Receiver: {},
          QuoteState: {},
          Request: {},
          DerivedVatRegime: {},
          DerivedIrpfRegime: {},
          ContactPerson: {},
          QuoteItems: {
            Product: {},
            QuoteItemState: {},
          },
          CreatedBy: {},
          LastModifiedBy: {},
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
        },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);

    if (this.object.Receiver) {
      this.previousReceiver = this.object.Receiver;
      this.update(this.object.Receiver);
    }
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .Receiver as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.object.ContactPerson = person;
  }

  public partyContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.Receiver;
    this.object.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public receiverSelected(party: IObject): void {
    if (party) {
      this.update(party as Party);
    }
  }

  public receiverAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.request.Originator = party;
  }

  private update(party: Party) {
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
      if (this.object.Receiver !== this.previousReceiver) {
        this.object.ContactPerson = null;
        this.object.FullfillContactMechanism = null;

        this.previousReceiver = this.object.Receiver;
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
