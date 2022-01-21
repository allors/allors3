import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  ProductIdentificationType,
  ProductIdentification,
  Part,
  Good,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './productidentification-edit.component.html',
  providers: [ContextService],
})
export class ProductIdentificationEditComponent implements OnInit, OnDestroy {
  public m: M;

  public title = 'Edit Good Identification';

  public container: IObject;
  public object: ProductIdentification;
  public productIdentificationTypes: ProductIdentificationType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ProductIdentificationEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
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
          const cls = this.data.strategy?.cls;
          const { associationRoleType } = this.data;

          const pulls = [
            pull.ProductIdentificationType({
              predicate: {
                kind: 'Equals',
                propertyType: m.ProductIdentificationType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.ProductIdentificationType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductIdentification({
                objectId: this.data.id,
                include: {
                  ProductIdentificationType: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Good({ objectId: this.data.associationId }),
              pull.Part({ objectId: this.data.associationId })
            );
          }

          return this.allors.context.pull(pulls).pipe(
            map((loaded) => ({
              loaded,
              create: isCreate,
              cls,
              associationRoleType,
            }))
          );
        })
      )
      .subscribe(({ loaded, create, cls, associationRoleType }) => {
        this.allors.context.reset();

        this.container =
          loaded.object<Good>(m.Good) || loaded.object<Part>(m.Part);
        this.object = loaded.object<ProductIdentification>(
          m.ProductIdentification
        );
        this.productIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );

        if (create) {
          this.title = 'Add Identification';
          this.object = this.allors.context.create<ProductIdentification>(cls);
          this.container.strategy.addCompositesRole(
            associationRoleType,
            this.object
          );
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
      this.dialogRef.close(this.object);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
