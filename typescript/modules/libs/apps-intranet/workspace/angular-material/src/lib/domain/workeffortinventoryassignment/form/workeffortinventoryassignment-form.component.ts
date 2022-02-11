import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Facility,
  InternalOrganisation,
  InventoryItem,
  NonSerialisedInventoryItem,
  NonSerialisedInventoryItemState,
  Part,
  SerialisedInventoryItem,
  SerialisedInventoryItemState,
  WorkEffort,
  WorkEffortInventoryAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortinventoryassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortInventoryAssignmentFormComponent extends AllorsFormComponent<WorkEffortInventoryAssignment> {
  readonly m: M;
  parts: Part[];
  workEffort: WorkEffort;
  inventoryItems: InventoryItem[];
  facility: Facility;
  state: NonSerialisedInventoryItemState | SerialisedInventoryItemState;
  serialised: boolean;

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
      p.InventoryItem({
        sorting: [{ roleType: m.InventoryItem.DisplayName }],
        include: {
          Part: {
            InventoryItemKind: {},
          },
          Facility: {},
          SerialisedInventoryItem_SerialisedInventoryItemState: {},
          NonSerialisedInventoryItem_NonSerialisedInventoryItemState: {},
        },
      }),
      p.Organisation({
        objectId: this.internalOrganisationId.value,
        name: 'InternalOrganisation',
        include: {
          FacilitiesWhereOwner: {},
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortInventoryAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            InventoryItem: {
              Part: {
                InventoryItemKind: {},
              },
            },
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

    const internalOrganisation = pullResult.object<InternalOrganisation>(
      this.m.InternalOrganisation
    );

    const inventoryItems = pullResult.collection<InventoryItem>(
      this.m.InventoryItem
    );
    this.inventoryItems = inventoryItems?.filter((v) =>
      internalOrganisation.FacilitiesWhereOwner.includes(v.Facility)
    );

    if (this.createRequest) {
      this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
      this.object.Assignment = this.workEffort;
    } else {
      this.workEffort = this.object.Assignment;
      this.inventoryItemSelected(this.object.InventoryItem);
    }
  }

  public inventoryItemSelected(inventoryItem: IObject): void {
    this.serialised =
      (inventoryItem as InventoryItem).Part.InventoryItemKind.UniqueId ===
      '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

    if (inventoryItem.strategy.cls === this.m.NonSerialisedInventoryItem) {
      const item = inventoryItem as NonSerialisedInventoryItem;
      this.state = item.NonSerialisedInventoryItemState;
    } else {
      const item = inventoryItem as SerialisedInventoryItem;
      this.state = item.SerialisedInventoryItemState;
    }
  }
}
