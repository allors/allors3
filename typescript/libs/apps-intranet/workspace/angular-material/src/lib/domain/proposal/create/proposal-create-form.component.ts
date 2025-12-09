import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  ContactMechanism,
  Currency,
  CustomerRelationship,
  InternalOrganisation,
  InvoiceItemType,
  IrpfRegime,
  NonUnifiedPart,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  Proposal,
  QuoteItem,
  SerialisedItem,
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
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './proposal-create-form.component.html',
  providers: [ContextService],
})
export class ProposalCreateFormComponent extends AllorsFormComponent<Proposal> {
  readonly m: M;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[] = [];
  contacts: Party[] = [];
  irpfRegimes: IrpfRegime[];
  vatRegimes: VatRegime[];
  showIrpf: boolean;

  addContactPerson = false;
  addContactMechanism = false;
  addReceiver = false;
  internalOrganisation: InternalOrganisation;
  private previousReceiver: Party;

  customersFilter: SearchFactory;
  currencyInitialRole: Currency;
  partItemType: InvoiceItemType;
  productItemType: InvoiceItemType;

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
      internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
      p.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] })
    );

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.InvoiceItemType({
          predicate: {
            kind: 'Equals',
            propertyType: m.InvoiceItemType.IsActive,
            value: true,
          },
        }),
        p.SerialisedItem({
          objectId: initializer.id,
          include: {
            PartWhereSerialisedItem: {},
          },
        }),
        p.NonUnifiedPart({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.currencies = pullResult.collection<Currency>(this.m.Currency);

    const invoiceItemTypes = pullResult.collection<InvoiceItemType>(
      this.m.InvoiceItemType
    );
    this.partItemType = invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b'
    );
    this.productItemType = invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad'
    );

    this.object.Issuer = this.internalOrganisation;
    this.object.IssueDate = new Date();
    this.object.ValidFromDate = new Date();

    const serialisedItem = pullResult.object<SerialisedItem>(
      this.m.SerialisedItem
    );

    const part = pullResult.object<NonUnifiedPart>(this.m.NonUnifiedPart);

    if (serialisedItem != null) {
      const quoteItem = this.allors.context.create<QuoteItem>(this.m.QuoteItem);

      quoteItem.InvoiceItemType = this.productItemType;
      quoteItem.SerialisedItem = serialisedItem;
      quoteItem.Product = serialisedItem.PartWhereSerialisedItem;
      quoteItem.Quantity = '1';
      quoteItem.AssignedUnitPrice = serialisedItem.ExpectedSalesPrice ?? '0';

      this.object.addQuoteItem(quoteItem);
    }

    if (part != null) {
      const quoteItem = this.allors.context.create<QuoteItem>(this.m.QuoteItem);

      quoteItem.InvoiceItemType = this.partItemType;
      quoteItem.Product = part;
      quoteItem.Quantity = '1';
      quoteItem.AssignedUnitPrice = '0';

      this.object.addQuoteItem(quoteItem);
    }
  }

  get receiverIsPerson(): boolean {
    return (
      !this.object.Receiver ||
      this.object.Receiver.strategy.cls === this.m.Person
    );
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

    this.object.Receiver = party;
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
        name: 'contacts',
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Party({
        object: party,
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          Locale: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousReceiver &&
        this.object.Receiver !== this.previousReceiver
      ) {
        this.object.ContactPerson = null;
        this.object.FullfillContactMechanism = null;
      }

      this.previousReceiver = this.object.Receiver;

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.contacts = loaded.collection<Party>('contacts') ?? [];

      const selectedParty = loaded.object<Party>('selectedParty');
      this.currencyInitialRole =
        selectedParty?.PreferredCurrency ??
        this.object.Issuer.PreferredCurrency;
    });
  }
}
