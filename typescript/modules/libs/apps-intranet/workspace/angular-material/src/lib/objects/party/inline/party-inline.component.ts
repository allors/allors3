import { Component, Output, EventEmitter } from '@angular/core';

import { Party } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  selector: 'party-party',
  templateUrl: './party-inline.component.html',
})
export class PartyInlineComponent {
  @Output() public saved: EventEmitter<Party> = new EventEmitter<Party>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  constructor(public allors: ContextService) {}
}
