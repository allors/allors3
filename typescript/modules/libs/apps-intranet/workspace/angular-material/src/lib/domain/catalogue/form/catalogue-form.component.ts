import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
} from '@allors/system/workspace/domain';
import {
  Catalogue,
  InternalOrganisation,
  Locale,
  ProductCategory,
  Scope,
  Singleton,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './catalogue-form.component.html',
  providers: [ContextService],
})
export class CatalogueFormComponent
  extends AllorsFormComponent<Catalogue>
  implements CreateOrEditPullHandler, EditIncludeHandler
{
  public m: M;
  public singleton: Singleton;
  public locales: Locale[];
  public categories: ProductCategory[];
  public scopes: Scope[];
  public internalOrganisation: InternalOrganisation;

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

    pulls.push(this.fetcher.locales);
    pulls.push(this.fetcher.categories),
      pulls.push(this.fetcher.locales),
      pulls.push(this.fetcher.internalOrganisation),
      pulls.push(p.Scope({}));
  }

  onEditInclude(): Node[] {
    const { treeBuilder: t } = this.m;

    return t.Catalogue({
      CatalogueImage: {},
      LocalisedNames: {
        Locale: {},
      },
      LocalisedDescriptions: {
        Locale: {},
      },
    });
  }

  onPostCreatePull(): void {
    this.object.InternalOrganisation = this.internalOrganisation;
  }

  onPostCreateOrEditPull(_, loaded: IPullResult): void {
    this.locales = this.fetcher.getAdditionalLocales(loaded);
    this.categories = this.fetcher.getProductCategories(loaded);
    this.scopes = loaded.collection<Scope>(this.m.Scope);
    this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
  }
}
