import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  CustomerRelationship,
  InternalOrganisation,
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
export class CustomerRelationshipFormComponent
  extends AllorsFormComponent<CustomerRelationship>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  internalOrganisation: InternalOrganisation;
  party: Party;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPreCreateOrEditPull(pulls: Pull[]): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.internalOrganisation), pulls.push(p.Scope({}));
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.m;

    return t.CustomerRelationship({
      InternalOrganisation: {},
    });
  }

  onPostCreatePull(_, loaded: IPullResult): void {
    this.object.FromDate = new Date();
    this.object.InternalOrganisation = this.internalOrganisation;

    this.party = loaded.object<Party>(this.m.Party);
    this.object.Customer = this.party;
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
  }

  // TODO: KOEN
  // Pre
  // if (isCreate && this.data.associationId) {
  //   pulls.push(
  //     pull.Party({
  //       object: this.data.associationId,
  //     }),
  //   );
  // }
}
