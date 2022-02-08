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
  BasePrice,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './baseprice-form.component.html',
  providers: [ContextService],
})
export class BasepriceFormComponent
  extends AllorsFormComponent<BasePrice>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  internalOrganisation: InternalOrganisation;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
  }

  onPreCreateOrEditPull(pulls: Pull[]): void {
    pulls.push(this.fetcher.internalOrganisation);
  }

  onEditInclude(): Node[] {
    // TODO: KOEN
    const { treeBuilder: t } = this.allors.workspaceService.workspace
      .configuration.metaPopulation as M;

    return t.BasePrice({
      Currency: {},
    });
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
  }

  onPostCreatePull(): void {
    this.object.FromDate = new Date();
    this.object.PricedBy = this.internalOrganisation;
  }

  // TODO: KOEN
  // Pre
  //         if (isCreate && this.data.associationId) {
  //           pulls = [
  //             ...pulls,
  //             pull.UnifiedGood({
  //               objectId: this.data.associationId,
  //             }),
  //           ];
  //         }
  // Post
  // if (this.nonUnifiedGood) {
  //   this.priceComponent.Product = this.unifiedGood;
  // }
}
