import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { PartyContactMechanism, PostalAddress, Country, ContactMechanismPurpose } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
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

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.Country({
        sorting: [{ roleType: this.m.Country.Name }],
      }),
      pull.ContactMechanismPurpose({
        predicate: { kind: 'Equals', propertyType: this.m.ContactMechanismPurpose.IsActive, value: true },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.countries = loaded.collection<Country>(m.Country);
      this.contactMechanismPurposes = loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);

      this.partyContactMechanism = this.allors.context.create<PartyContactMechanism>(m.PartyContactMechanism);
      this.postalAddress = this.allors.context.create<PostalAddress>(m.PostalAddress);
      this.partyContactMechanism.ContactMechanism = this.postalAddress;
    });
  }

  public ngOnDestroy(): void {
    if (this.partyContactMechanism) {
      this.partyContactMechanism.strategy.delete();
      this.postalAddress.strategy.delete();
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
