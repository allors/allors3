import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  CustomerRelationship,
  InternalOrganisation,
  Organisation,
  Party,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './customerrelationship-form.component.html',
  providers: [ContextService],
})
export class CustomerRelationshipFormComponent extends AllorsFormComponent<CustomerRelationship> {
  readonly m: M;

  internalOrganisation: InternalOrganisation;
  organisation: Organisation;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.internalOrganisation);

    if (this.editRequest) {
      pulls.push(
        p.CustomerRelationship({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            InternalOrganisation: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Organisation({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.organisation = pullResult.object<Organisation>(this.m.Organisation);
    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);

    if (this.createRequest) {
      this.object.FromDate = new Date();
      this.object.Customer = this.organisation;
      this.object.InternalOrganisation = this.internalOrganisation;
    }
  }
}
