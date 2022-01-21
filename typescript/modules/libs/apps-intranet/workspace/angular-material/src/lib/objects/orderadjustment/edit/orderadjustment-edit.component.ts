import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Invoice,
  Order,
  OrderAdjustment,
  Quote,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './orderadjustment-edit.component.html',
  providers: [ContextService],
})
export class OrderAdjustmentEditComponent implements OnInit, OnDestroy {
  public m: M;

  public title: string;

  public container: IObject;
  public object: OrderAdjustment;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<OrderAdjustmentEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const cls = this.data.strategy?.cls;
          const { associationRoleType } = this.data;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.OrderAdjustment({
                objectId: this.data.id,
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Quote({ objectId: this.data.associationId }),
              pull.Order({ objectId: this.data.associationId }),
              pull.Invoice({ objectId: this.data.associationId })
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
          loaded.object<Quote>(m.Quote) ??
          loaded.object<Order>(m.Order) ??
          loaded.object<Invoice>(m.Invoice);
        this.object = loaded.object<OrderAdjustment>(m.OrderAdjustment);

        if (create) {
          this.title = `Add ${cls.singularName}`;
          this.object = this.allors.context.create<OrderAdjustment>(cls);
          this.container.strategy.addCompositesRole(
            associationRoleType,
            this.object
          );
        } else {
          this.title = `Edit ${this.object.strategy.cls.singularName}`;
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
