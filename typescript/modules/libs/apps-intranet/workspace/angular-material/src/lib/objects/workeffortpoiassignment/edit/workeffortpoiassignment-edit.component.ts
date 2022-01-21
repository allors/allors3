import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  WorkEffort,
  WorkEffortPurchaseOrderItemAssignment,
  PurchaseOrder,
  PurchaseOrderItem,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortpoiassignment-edit.component.html',
  providers: [ContextService],
})
export class WorkEffortPurchaseOrderItemAssignmentEditComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  title: string;
  workEffortPurchaseOrderItemAssignment: WorkEffortPurchaseOrderItemAssignment;
  workEffort: WorkEffort;
  selectedPurchaseOrder: PurchaseOrder;

  private subscription: Subscription;
  purchaseOrders: PurchaseOrder[];
  purchaseOrderItems: PurchaseOrderItem[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortPurchaseOrderItemAssignmentEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    private snackBar: MatSnackBar
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

          let pulls = [
            pull.PurchaseOrder({
              sorting: [{ roleType: this.m.PurchaseOrder.OrderNumber }],
              include: {
                TakenViaSupplier: x,
                PurchaseOrderItems: {
                  Part: x,
                  PurchaseOrderWherePurchaseOrderItem: x,
                },
                WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder: x,
              },
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortPurchaseOrderItemAssignment({
                objectId: this.data.id,
                include: {
                  Assignment: x,
                  PurchaseOrderItem: x,
                },
              })
            );
          }

          if (isCreate) {
            pulls = [
              ...pulls,
              pull.WorkEffort({
                objectId: this.data.associationId,
              }),
            ];
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        if (isCreate) {
          this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
          this.title = 'Add purchase order item assignment';

          this.workEffortPurchaseOrderItemAssignment =
            this.allors.context.create<WorkEffortPurchaseOrderItemAssignment>(
              m.WorkEffortPurchaseOrderItemAssignment
            );
          this.workEffortPurchaseOrderItemAssignment.Assignment =
            this.workEffort;
          this.workEffortPurchaseOrderItemAssignment.Quantity = 1;
        } else {
          this.workEffortPurchaseOrderItemAssignment =
            loaded.object<WorkEffortPurchaseOrderItemAssignment>(
              m.WorkEffortPurchaseOrderItemAssignment
            );
          this.selectedPurchaseOrder =
            this.workEffortPurchaseOrderItemAssignment.PurchaseOrder;
          this.workEffort =
            this.workEffortPurchaseOrderItemAssignment.Assignment;

          if (
            this.workEffortPurchaseOrderItemAssignment.canWritePurchaseOrderItem
          ) {
            this.title = 'Edit purchase order item assignment';
          } else {
            this.title = 'View purchase order item assignment';
          }
        }

        const purchaseOrders = loaded.collection<PurchaseOrder>(
          m.PurchaseOrder
        );
        this.purchaseOrders = purchaseOrders?.filter((v) =>
          v.PurchaseOrderItems?.find(
            (i) =>
              i.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem
                .length === 0 &&
              !i.Part &&
              i.PurchaseOrderWherePurchaseOrderItem.OrderedBy ===
                this.workEffort.TakenBy
          )
        );
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public update(): void {
    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workEffortPurchaseOrderItemAssignment);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public purchaseOrderSelected(purchaseOrder: PurchaseOrder): void {
    this.purchaseOrderItems = purchaseOrder.PurchaseOrderItems?.filter(
      (v) =>
        v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem
          .length === 0
    );
  }
}
