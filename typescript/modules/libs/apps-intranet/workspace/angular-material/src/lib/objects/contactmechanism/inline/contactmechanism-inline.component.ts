import { Component, Output, EventEmitter } from '@angular/core';

import { PartyContactMechanism } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'party-contactmechanism',
  templateUrl: './contactmechanism-inline.component.html',
})
export class ContactMechanismInlineComponent {
  @Output() public saved: EventEmitter<PartyContactMechanism> =
    new EventEmitter<PartyContactMechanism>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  constructor(public allors: ContextService) {}
}
