import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
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
export class CatalogueFormComponent extends AllorsFormComponent<Catalogue> {
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

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.locales);
    pulls.push(this.fetcher.categories),
      pulls.push(this.fetcher.locales),
      pulls.push(this.fetcher.internalOrganisation),
      pulls.push(p.Scope({}));

    if (this.editRequest) {
      pulls.push(
        p.Catalogue({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            CatalogueImage: {},
            LocalisedNames: {
              Locale: {},
            },
            LocalisedDescriptions: {
              Locale: {},
            },
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation = this.fetcher.getInternalOrganisation(pullResult);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.categories = this.fetcher.getProductCategories(pullResult);
    this.scopes = pullResult.collection<Scope>(this.m.Scope);

    this.object.InternalOrganisation = this.internalOrganisation;
  }
}
