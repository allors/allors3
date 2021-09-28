import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { PartyContactMechanism, ContactMechanismPurpose, WebAddress } from '@allors/workspace/domain/default';
import { SessionService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'party-contactmechanism-webAddress',
  templateUrl: './webaddress-inline.component.html',
})
export class InlineWebAddressComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<PartyContactMechanism> = new EventEmitter<PartyContactMechanism>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public contactMechanismPurposes: ContactMechanismPurpose[];

  public partyContactMechanism: PartyContactMechanism;
  public webAddress: WebAddress;

  public m: M;

  constructor(private allors: SessionService) {
    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.ContactMechanismPurpose({
        predicate: { kind: 'Equals', propertyType: this.m.ContactMechanismPurpose.IsActive, value: true },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      }),
    ];

    this.allors.client.pullReactive(this.allors.session, pulls).subscribe(
      (loaded) => {
        this.contactMechanismPurposes = loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);
        this.partyContactMechanism = this.allors.session.create<PartyContactMechanism>(m.PartyContactMechanism);
        this.webAddress = this.allors.session.create<WebAddress>(m.WebAddress);
        this.partyContactMechanism.ContactMechanism = this.webAddress;
      },
      (error: any) => {
        this.cancelled.emit();
      }
    );
  }

  public ngOnDestroy(): void {
    if (!!this.partyContactMechanism) {
      this.allors.client.invokeReactive(this.allors.session, this.partyContactMechanism.Delete);
      this.allors.client.invokeReactive(this.allors.session, this.webAddress.Delete);
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.partyContactMechanism);

    this.partyContactMechanism = undefined;
    this.webAddress = undefined;
  }
}
