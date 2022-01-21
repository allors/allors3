import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { ProductType, SerialisedItemCharacteristicType } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './producttype-edit.component.html',
  providers: [ContextService],
})
export class ProductTypeEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public productType: ProductType;

  public characteristics: SerialisedItemCharacteristicType[];

  private subscription: Subscription;

  constructor(@Self() public allors: ContextService, @Inject(MAT_DIALOG_DATA) public data: ObjectData, public dialogRef: MatDialogRef<ProductTypeEditComponent>, public refreshService: RefreshService, private saveService: SaveService) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            pull.SerialisedItemCharacteristicType({
              sorting: [{ roleType: m.SerialisedItemCharacteristicType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductType({
                objectId: this.data.id,
                include: {
                  SerialisedItemCharacteristicTypes: x,
                },
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.characteristics = loaded.collection<SerialisedItemCharacteristicType>(m.SerialisedItemCharacteristicType);

        if (isCreate) {
          this.title = 'Add Product Type';
          this.productType = this.allors.context.create<ProductType>(m.ProductType);
        } else {
          this.productType = loaded.object<ProductType>(m.ProductType);

          if (this.productType.canWriteName) {
            this.title = 'Edit Product Type';
          } else {
            this.title = 'View Product Type';
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
      this.dialogRef.close(this.productType);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
