import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Locale, PartCategory } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './partcategory-edit.component.html',
  providers: [ContextService],
})
export class PartCategoryEditComponent extends TestScope implements OnInit, OnDestroy {
  public m: M;
  public title: string;

  public category: PartCategory;
  public locales: Locale[];
  public categories: PartCategory[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PartCategoryEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.locales,
            pull.PartCategory({
              sorting: [{ roleType: m.PartCategory.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PartCategory({
                objectId: this.data.id,
                include: {
                  CategoryImage: x,
                  Children: x,
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

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.category = loaded.object<PartCategory>(m.PartCategory);
        this.categories = loaded.collection<PartCategory>(m.PartCategory);
        this.locales = this.fetcher.getAdditionalLocales(loaded);

        if (isCreate) {
          this.title = 'Add Part Category';
          this.category = this.allors.context.create<PartCategory>(m.PartCategory);
        } else {
          if (this.category.canWriteName) {
            this.title = 'Edit Part Category';
          } else {
            this.title = 'View Part Category';
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
      this.dialogRef.close(this.category);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
