import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  InvoiceItemType,
  WorkEffort,
  WorkEffortInvoiceItem,
  WorkEffortInvoiceItemAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './workeffortinvoiceassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortInvoiceItemAssignmentFormComponent extends AllorsFormComponent<WorkEffortInvoiceItemAssignment> {
  readonly m: M;
  workEffort: WorkEffort;
  internalOrganisation: InternalOrganisation;
  workEffortInvoiceItem: WorkEffortInvoiceItem;
  invoiceItemTypes: InvoiceItemType[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.InvoiceItemType({
        predicate: {
          kind: 'Equals',
          propertyType: m.InvoiceItemType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.InvoiceItemType.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortInvoiceItemAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            WorkEffortInvoiceItem: {},
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

    this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
    this.invoiceItemTypes = pullResult.collection<InvoiceItemType>(
      this.m.InvoiceItemType
    );

    if (this.createRequest) {
      this.object.Assignment = this.workEffort;

      this.workEffortInvoiceItem =
        this.allors.context.create<WorkEffortInvoiceItem>(
          this.m.WorkEffortInvoiceItem
        );
      this.object.WorkEffortInvoiceItem = this.workEffortInvoiceItem;
    } else {
      this.workEffortInvoiceItem = this.object.WorkEffortInvoiceItem;
    }
  }
}
