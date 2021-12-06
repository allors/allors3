import { Component, Output, EventEmitter } from '@angular/core';

import { PartyContactMechanism } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'party-contactmechanism',
  templateUrl: './contactmechanism-inline.component.html',
})
export class ContactMechanismInlineComponent {
  @Output() public saved: EventEmitter<PartyContactMechanism> = new EventEmitter<PartyContactMechanism>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  constructor(public allors: ContextService) {}
}
