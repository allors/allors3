import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  Input,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  PartyContactMechanism,
  ContactMechanismPurpose,
  EmailAddress,
} from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  selector: 'party-contactmechanism-emailAddress',
  templateUrl: './emailaddress-inline.component.html',
})
export class PartyContactMechanismEmailAddressInlineComponent
  implements OnInit, OnDestroy
{
  @Output() public saved: EventEmitter<PartyContactMechanism> =
    new EventEmitter<PartyContactMechanism>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  @Input() public scope: ContextService;

  public contactMechanismPurposes: ContactMechanismPurpose[];

  public partyContactMechanism: PartyContactMechanism;
  public emailAddress: EmailAddress;

  public m: M;

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.ContactMechanismPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.ContactMechanismPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe(
      (loaded) => {
        this.contactMechanismPurposes =
          loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);

        this.partyContactMechanism =
          this.allors.context.create<PartyContactMechanism>(
            m.PartyContactMechanism
          );
        this.emailAddress = this.allors.context.create<EmailAddress>(
          m.EmailAddress
        );
        this.partyContactMechanism.ContactMechanism = this.emailAddress;
      },
      (error: any) => {
        this.cancelled.emit();
      }
    );
  }

  public ngOnDestroy(): void {
    if (this.partyContactMechanism) {
      this.partyContactMechanism.strategy.delete();
      this.emailAddress.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.partyContactMechanism);

    this.partyContactMechanism = undefined;
    this.emailAddress = undefined;
  }
}
