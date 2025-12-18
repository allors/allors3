import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import {
  PartyContactMechanism,
  Enumeration,
  ContactMechanismPurpose,
  ContactMechanismType,
  TelecommunicationsNumber,
} from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'party-contactmechanism-telecommunicationsnumber',
  templateUrl: './telecommunicationsnumber-inline.component.html',
})
export class PartyContactMechanismTelecommunicationsNumberInlineComponent
  implements OnInit, OnDestroy
{
  @Output()
  public saved: EventEmitter<PartyContactMechanism> =
    new EventEmitter<PartyContactMechanism>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public contactMechanismPurposes: Enumeration[];
  public contactMechanismTypes: ContactMechanismType[];

  public partyContactMechanism: PartyContactMechanism;
  public telecommunicationsNumber: TelecommunicationsNumber;

  public m: M;

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.ContactMechanismPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.ContactMechanismPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
      }),
      pull.ContactMechanismType({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.ContactMechanismType.IsActive,
          value: true,
        },
        sorting: [{ roleType: this.m.ContactMechanismType.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.contactMechanismPurposes =
        loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);
      this.contactMechanismTypes = loaded.collection<ContactMechanismType>(
        m.ContactMechanismType
      );

      this.partyContactMechanism =
        this.allors.context.create<PartyContactMechanism>(
          m.PartyContactMechanism
        );
      this.telecommunicationsNumber =
        this.allors.context.create<TelecommunicationsNumber>(
          m.TelecommunicationsNumber
        );
      this.partyContactMechanism.ContactMechanism =
        this.telecommunicationsNumber;
    });
  }

  public ngOnDestroy(): void {
    if (this.partyContactMechanism) {
      this.partyContactMechanism.strategy.delete();
      this.telecommunicationsNumber.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.partyContactMechanism);

    this.partyContactMechanism = undefined;
    this.telecommunicationsNumber = undefined;
  }
}
