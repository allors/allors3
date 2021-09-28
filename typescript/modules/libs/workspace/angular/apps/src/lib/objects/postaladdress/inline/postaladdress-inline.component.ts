import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Carrier,  Person, Organisation, PartyContactMechanism, OrganisationContactRelationship, Party, CustomerShipment, Currency, PostalAddress, Facility, ShipmentMethod, PositionTypeRate, TimeFrequency, RateType, PositionType, PriceComponent, Country, ContactMechanismPurpose } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, SaveService, SearchFactory, Table, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'party-contactmechanism-postaladdress',
  templateUrl: './postaladdress-inline.component.html',
})
export class PartyContactMechanismPostalAddressInlineComponent implements OnInit, OnDestroy {

  @Output()
  public saved: EventEmitter<PartyContactMechanism> = new EventEmitter<PartyContactMechanism>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public countries: Country[];
  public contactMechanismPurposes: ContactMechanismPurpose[];

  public partyContactMechanism: PartyContactMechanism;
  public postalAddress: PostalAddress;

  public m: M;

  constructor(
    private allors: SessionService) {

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const { pullBuilder: pull } = this.m;

    const pulls = [
      pull.Country({
        sorting: [{ roleType: this.m.Country.Name }]
      }),
      pull.ContactMechanismPurpose({
        predicate: { kind: 'Equals', propertyType: this.m.ContactMechanismPurpose.IsActive, value: true },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }]
      })
    ];

    this.allors.context
      .load(new PullRequest({ pulls }))
      .subscribe((loaded) => {
        this.countries = loaded.collection<Country>(m.Country);
        this.contactMechanismPurposes = loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);

        this.partyContactMechanism = this.allors.session.create<PartyContactMechanism>(m.PartyContactMechanism);
        this.postalAddress = this.allors.session.create<PostalAddress>(m.PostalAddress);
        this.partyContactMechanism.ContactMechanism = this.postalAddress;
      });
  }

  public ngOnDestroy(): void {

    if (this.partyContactMechanism) {
      this.allors.client.invokeReactive(this.allors.session, this.partyContactMechanism.Delete);
      this.allors.client.invokeReactive(this.allors.session, this.postalAddress.Delete);
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.partyContactMechanism);

    this.partyContactMechanism = undefined;
    this.postalAddress = undefined;
  }
}
