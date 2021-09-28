import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, RefreshService, Saved, MetaService } from '@allors/angular/services/core';
import { Meta } from '@allors/meta/generated';
import { PartCategory, Locale } from '@allors/domain/generated';
import { ObjectData, SaveService } from '@allors/angular/material/services/core';
import { FetcherService, InternalOrganisationId } from '@allors/angular/base';
import { Sort } from '@allors/data/system';
import { PullRequest } from '@allors/protocol/system';
import { IObject } from '@allors/domain/system';
import { TestScope, Action } from '@allors/angular/core';

@Component({
  templateUrl: './partcategory-edit.component.html',
  providers: [SessionService]
})
export class PartCategoryEditComponent extends TestScope implements OnInit, OnDestroy {

  public m: M;
  public title: string;

  public category: PartCategory;
  public locales: Locale[];
  public categories: PartCategory[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PartCategoryEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          const pulls = [
            this.fetcher.locales,
            pull.PartCategory({
              sorting: [{ roleType: m.PartCategory.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PartCategory(
                {
                  objectId: this.data.id,
                  include: {
                    Children: x,
                    LocalisedNames: {
                      Locale: x,
                    },
                    LocalisedDescriptions: {
                      Locale: x,
                    }
                  }
                }
              ),
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls)
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {

        this.allors.session.reset();

        this.category = loaded.object<PartCategory>(m.PartCategory);
        this.categories = loaded.collection<PartCategory>(m.PartCategory);
        this.locales = loaded.collection<Locale>(m.Locale);

        if (isCreate) {
          this.title = 'Add Part Category';
          this.category = this.allors.session.create<PartCategory>(m.PartCategory);
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

    this.allors.context
      .save()
      .subscribe((saved: Saved) => {
        const data: IObject = {
          id: this.category.id,
          objectType: this.category.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
