import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  AssetAssignmentStatus,
  Enumeration,
  Organisation,
  Party,
  SerialisedItem,
  WorkEffort,
  WorkEffortFixedAssetAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './workeffortfixedassetassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortFixedAssetAssignmentFormComponent extends AllorsFormComponent<WorkEffortFixedAssetAssignment> {
  readonly m: M;
  workEffort: WorkEffort;
  assignment: WorkEffort;
  serialisedItem: SerialisedItem;
  assetAssignmentStatuses: Enumeration[];
  serialisedItems: SerialisedItem[];
  externalCustomer: boolean;

  serialisedItemsFilter: SearchFactory;
  workEfforts: WorkEffort[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.serialisedItemsFilter = Filters.serialisedItemsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.WorkEffort({
        sorting: [{ roleType: m.WorkEffort.Name }],
      }),
      p.AssetAssignmentStatus({
        predicate: {
          kind: 'Equals',
          propertyType: m.AssetAssignmentStatus.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.AssetAssignmentStatus.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortFixedAssetAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            FixedAsset: {},
            AssetAssignmentStatus: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.SerialisedItem({
          objectId: initializer.id,
          sorting: [{ roleType: m.SerialisedItem.Name }],
        }),
        p.WorkEffort({
          objectId: initializer.id,
          include: { Customer: {} },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
    this.workEfforts = pullResult.collection<WorkEffort>(this.m.WorkEffort);
    this.serialisedItem = pullResult.object<SerialisedItem>(
      this.m.SerialisedItem
    );
    this.assetAssignmentStatuses = pullResult.collection<AssetAssignmentStatus>(
      this.m.AssetAssignmentStatus
    );

    if (this.serialisedItem == null) {
      const b2bCustomer = this.workEffort.Customer as Organisation;
      this.externalCustomer =
        b2bCustomer == null || !b2bCustomer.IsInternalOrganisation;

      if (this.externalCustomer) {
        this.updateSerialisedItems(this.workEffort.Customer);
      }
    }

    if (this.createRequest) {
      if (this.serialisedItem != null) {
        this.object.FixedAsset = this.serialisedItem;
      }

      if (
        this.workEffort != null &&
        this.workEffort.strategy.cls === this.m.WorkTask
      ) {
        this.assignment = this.workEffort as WorkEffort;
        this.object.Assignment = this.assignment;
      }
    }
  }

  private updateSerialisedItems(customer: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: customer,
        select: {
          SerialisedItemsWhereOwnedBy: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.serialisedItems = loaded.collection<SerialisedItem>(
        m.Party.SerialisedItemsWhereOwnedBy
      );
    });
  }
}
