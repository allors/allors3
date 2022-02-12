import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  PurchaseOrder,
  PurchaseOrderItem,
  WorkEffort,
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
export class WorkEffortPurchaseOrderItemAssignmentFormComponent extends AllorsFormComponent<WorkEffortPurchaseOrderItemAssignment> {
  readonly m: M;
  workEffort: WorkEffort;
  selectedPurchaseOrder: PurchaseOrder;
  purchaseOrders: PurchaseOrder[];
  purchaseOrderItems: PurchaseOrderItem[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.PurchaseOrder({
        sorting: [{ roleType: this.m.PurchaseOrder.OrderNumber }],
        include: {
          TakenViaSupplier: {},
          PurchaseOrderItems: {
            Part: {},
            PurchaseOrderWherePurchaseOrderItem: {},
          },
          WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder: {},
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortPurchaseOrderItemAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            PurchaseOrderItem: {},
          },
        })
      );
    }

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffort({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    if (this.createRequest) {
      this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);

      this.object.Assignment = this.workEffort;
      this.object.Quantity = 1;
    } else {
      this.selectedPurchaseOrder = this.object.PurchaseOrder;
      this.workEffort = this.object.Assignment;
    }

    const purchaseOrders = pullResult.collection<PurchaseOrder>(
      this.m.PurchaseOrder
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
  }

  public purchaseOrderSelected(purchaseOrder: PurchaseOrder): void {
    this.purchaseOrderItems = purchaseOrder.PurchaseOrderItems?.filter(
      (v) =>
        v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem
          .length === 0
    );
  }
}
