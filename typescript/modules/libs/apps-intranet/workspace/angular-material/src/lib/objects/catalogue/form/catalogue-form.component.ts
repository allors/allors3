import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  InternalOrganisation,
  Locale,
  Catalogue,
  Singleton,
  ProductCategory,
  Scope,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './catalogue-edit.component.html',
  providers: [ContextService],
})
export class CatalogueEditComponent implements OnInit, OnDestroy {
  public m: M;

  public catalogue: Catalogue;
  public title: string;

  public subTitle: string;

  public singleton: Singleton;
  public locales: Locale[];
  public categories: ProductCategory[];
  public scopes: Scope[];
  public internalOrganisation: InternalOrganisation;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CatalogueEditComponent>,

    private refreshService: RefreshService,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.categories,
            this.fetcher.locales,
            this.fetcher.internalOrganisation,
            pull.Scope({}),
          ];

          if (!isCreate) {
            pulls.push(
              pull.Catalogue({
                objectId: this.data.id,
                include: {
                  CatalogueImage: x,
                  LocalisedNames: {
                    Locale: x,
                  },
                  LocalisedDescriptions: {
                    Locale: x,
                  },
                },
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, create: isCreate })));
        })
      )
      .subscribe(({ loaded, create }) => {
        this.allors.context.reset();

        this.catalogue = loaded.object<Catalogue>(m.Catalogue);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.categories = this.fetcher.getProductCategories(loaded);
        this.scopes = loaded.collection<Scope>(m.Scope);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);

        if (create) {
          this.title = 'Add Catalogue';
          this.catalogue = this.allors.context.create<Catalogue>(m.Catalogue);
          this.catalogue.InternalOrganisation = this.internalOrganisation;
        } else {
          if (this.catalogue.canWriteCatScope) {
            this.title = 'Edit Catalogue';
          } else {
            this.title = 'View Catalogue';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.catalogue);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
