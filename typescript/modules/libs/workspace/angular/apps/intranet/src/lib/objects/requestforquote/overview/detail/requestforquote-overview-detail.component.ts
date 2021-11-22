import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, OrganisationContactRelationship, Party, InternalOrganisation, ContactMechanism, PartyContactMechanism, Currency, RequestForQuote, Quote, CustomerRelationship } from '@allors/workspace/domain/default';
import { PanelService, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'requestforquote-overview-detail',
  templateUrl: './requestforquote-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class RequestForQuoteOverviewDetailComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  request: RequestForQuote;
  quote: Quote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[] = [];
  contacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  addContactPerson = false;
  addContactMechanism = false;
  addOriginator = false;
  previousOriginator: Party;

  private subscription: Subscription;

  customersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Request For Quote Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const requestForQuotePullName = `${panel.name}_${this.m.RequestForQuote.tag}`;
    const productQuotePullName = `${panel.name}_${this.m.ProductQuote.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};

        pulls.push(
          pull.RequestForQuote({
            name: requestForQuotePullName,
            objectId: this.panel.manager.id,
            include: {
              FullfillContactMechanism: {
                PostalAddress_Country: x,
              },
              RequestItems: {
                Product: x,
              },
              Originator: x,
              ContactPerson: x,
              RequestState: x,
              DerivedCurrency: x,
              CreatedBy: x,
              LastModifiedBy: x,
            },
          }),
          pull.RequestForQuote({
            name: productQuotePullName,
            objectId: this.panel.manager.id,
            select: {
              QuoteWhereRequest: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.request = loaded.object<RequestForQuote>(requestForQuotePullName);
        this.quote = loaded.object<Quote>(productQuotePullName);
      }
    };
  }

  public ngOnInit(): void {
    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.request = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
            pull.RequestForQuote({
              objectId: id,
              include: {
                DerivedCurrency: x,
                Originator: x,
                ContactPerson: x,
                RequestState: x,
                FullfillContactMechanism: {
                  PostalAddress_Country: x,
                },
              },
            }),
          ];

          this.customersFilter = Filters.customersFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.request = loaded.object<RequestForQuote>(this.m.RequestForQuote);
        this.currencies = loaded.collection<Currency>(this.m.Currency);

        if (this.request.Originator) {
          this.previousOriginator = this.request.Originator;
          this.update(this.request.Originator);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }

  get originatorIsPerson(): boolean {
    return !this.request.Originator || this.request.Originator.strategy.cls === this.m.Person;
  }

  public originatorSelected(party: IObject) {
    if (party) {
      this.update(party as Party);
    }
  }

  public partyContactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.request.Originator.addPartyContactMechanism(partyContactMechanism);
    this.request.FullfillContactMechanism = partyContactMechanism.ContactMechanism;
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.request.Originator as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.request.ContactPerson = person;
  }

  public originatorAdded(party: Party): void {
    const customerRelationship = this.allors.context.create<CustomerRelationship>(this.m.CustomerRelationship);
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
        objectId: party.id,
        select: {
          PartyContactMechanisms: x,
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
        objectId: party.id,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.request.Originator !== this.previousOriginator) {
        this.request.FullfillContactMechanism = null;
        this.request.ContactPerson = null;
        this.previousOriginator = this.request.Originator;
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.contactMechanisms = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);
      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
