import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  EmailMessage,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './emailmessage-form.component.html',
  providers: [ContextService],
})
export class EmailMessageFormComponent extends AllorsFormComponent<EmailMessage> {
  public m: M;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.EmailMessage({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          Sender: {},
          Recipients: {},
        },
      })
    );
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');
  }
}
