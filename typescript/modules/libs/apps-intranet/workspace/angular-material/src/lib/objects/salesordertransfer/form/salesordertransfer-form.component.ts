import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  SalesInvoice,
  SalesOrder,
  SalesTerm,
  TermType,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './salesordertransfer-edit.component.html',
  providers: [ContextService],
})
export class SalesOrderTransferEditComponent implements OnInit, OnDestroy {
  public m: M;

  public title = 'Edit Term Type';

  public container: IObject;
  public object: SalesTerm;
  public termTypes: TermType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SalesOrderTransferEditComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService
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
            pull.SalesTerm({
              objectId: this.data.id,
              include: {
                TermType: x,
              },
            }),
            pull.TermType({
              predicate: {
                kind: 'Equals',
                propertyType: m.TermType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.TermType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.SalesTerm({
                objectId: this.data.id,
                include: {
                  TermType: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.SalesInvoice({ objectId: this.data.associationId }),
              pull.SalesOrder({ objectId: this.data.associationId })
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
          loaded.object<SalesInvoice>(m.SalesInvoice) ||
          loaded.object<SalesOrder>(m.SalesOrder);
        this.object = loaded.object<SalesTerm>(m.SalesTerm);
        this.termTypes = loaded.collection<TermType>(m.TermType);
        this.termTypes = this.termTypes?.filter(
          (v) => v.strategy.cls.singularName === `${cls.singularName}Type`
        );

        if (create) {
          this.title = 'Add Sales Term';
          this.object = this.allors.context.create<SalesTerm>(cls);
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
    }, this.errorService.errorHandler);
  }
}
