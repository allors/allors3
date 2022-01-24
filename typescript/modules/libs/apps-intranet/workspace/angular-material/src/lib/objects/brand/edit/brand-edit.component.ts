import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { Brand, Model, Locale } from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './brand-edit.component.html',
  providers: [ContextService],
})
export class BrandEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public brand: Brand;
  locales: Locale[];
  addModel = false;

  private subscription: Subscription;
  models: Model[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<BrandEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [this.fetcher.locales];

          if (!isCreate) {
            pulls.push(
              pull.Brand({
                objectId: this.data.id,
                include: {
                  LogoImage: x,
                  Models: x,
                  LocalisedDescriptions: x,
                },
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();
        this.locales = this.fetcher.getAdditionalLocales(loaded);

        if (isCreate) {
          this.title = 'Add Brand';
          this.brand = this.allors.context.create<Brand>(m.Brand);
        } else {
          this.brand = loaded.object<Brand>(m.Brand);

          if (this.brand.canWriteName) {
            this.title = 'Edit Brand';
          } else {
            this.title = 'View Brand';
          }
        }

        this.models = this.brand.Models.sort((a, b) =>
          a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
        );
      });
  }

  public modelAdded(model: Model): void {
    this.brand.addModel(model);
    this.models = this.brand.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.brand);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
