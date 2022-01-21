import { SessionState } from '@allors/workspace/angular/base';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerOrganisationId extends SessionState {
  constructor() {
    super('State$InternalOrganisationId');
  }
}
