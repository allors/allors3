import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  WorkEffortPurchaseOrderItemAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortpoiassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortPurchaseOrderItemAssignmentFormComponent
  extends AllorsFormComponent<WorkEffortPurchaseOrderItemAssignment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
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
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId,
    private snackBar: MatSnackBar
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
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

  public purchaseOrderSelected(purchaseOrder: PurchaseOrder): void {
    this.purchaseOrderItems = purchaseOrder.PurchaseOrderItems?.filter(
      (v) =>
        v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem
          .length === 0
    );
  }
}
