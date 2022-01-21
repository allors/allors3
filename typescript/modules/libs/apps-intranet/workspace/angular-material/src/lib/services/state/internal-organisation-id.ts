import { SessionState } from '@allors/base/workspace/angular/foundation';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InternalOrganisationId extends SessionState {
  constructor() {
    super('State$InternalOrganisationId');
  }
}
