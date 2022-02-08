import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
  IObject,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  Brand,
  InternalOrganisation,
  Model,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './brand-form.component.html',
  providers: [ContextService],
})
export class BrandFormComponent
  extends AllorsFormComponent<Brand>
  implements CreateOrEditPullHandler, EditIncludeHandler
{
  locales: Locale[];
  addModel = false;
  models: Model[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
  }

  onPreCreateOrEditPull(pulls: Pull[]): void {
    pulls.push(this.fetcher.locales);
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.allors.workspaceService.workspace
      .configuration.metaPopulation as M;

    return t.Brand({
      LogoImage: {},
      Models: {},
      LocalisedDescriptions: {},
    });
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.locales = this.fetcher.getAdditionalLocales(loaded);

    this.models = this.object.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
  }

  public modelAdded(model: Model): void {
    this.object.addModel(model);
    this.models = this.object.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
  }
}
