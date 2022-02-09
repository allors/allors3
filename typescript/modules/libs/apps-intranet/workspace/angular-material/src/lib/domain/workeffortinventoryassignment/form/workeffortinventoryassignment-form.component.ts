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
export class WorkEffortInventoryAssignmentFormComponent
  extends AllorsFormComponent<WorkEffortInventoryAssignment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  title: string;
  workEffortInventoryAssignment: WorkEffortInventoryAssignment;
  parts: Part[];
  workEffort: WorkEffort;
  inventoryItems: InventoryItem[];
  facility: Facility;
  state: NonSerialisedInventoryItemState | SerialisedInventoryItemState;
  serialised: boolean;

  private subscription: Subscription;

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

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          let pulls = [
            pull.InventoryItem({
              sorting: [{ roleType: m.InventoryItem.DisplayName }],
              include: {
                Part: {
                  InventoryItemKind: x,
                },
                Facility: x,
                SerialisedInventoryItem_SerialisedInventoryItemState: x,
                NonSerialisedInventoryItem_NonSerialisedInventoryItemState: x,
              },
            }),
            pull.Organisation({
              objectId: this.internalOrganisationId.value,
              name: 'InternalOrganisation',
              include: {
                FacilitiesWhereOwner: x,
              },
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortInventoryAssignment({
                objectId: this.data.id,
                include: {
                  Assignment: x,
                  InventoryItem: {
                    Part: {
                      InventoryItemKind: x,
                    },
                  },
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
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

        const internalOrganisation = loaded.object<InternalOrganisation>(
          m.InternalOrganisation
        );

        const inventoryItems = loaded.collection<InventoryItem>(
          m.InventoryItem
        );
        this.inventoryItems = inventoryItems?.filter((v) =>
          internalOrganisation.FacilitiesWhereOwner.includes(v.Facility)
        );

        if (isCreate) {
          this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);

          this.title = 'Add inventory assignment';

          this.workEffortInventoryAssignment =
            this.allors.context.create<WorkEffortInventoryAssignment>(
              m.WorkEffortInventoryAssignment
            );
          this.workEffortInventoryAssignment.Assignment = this.workEffort;
        } else {
          this.workEffortInventoryAssignment =
            loaded.object<WorkEffortInventoryAssignment>(
              m.WorkEffortInventoryAssignment
            );
          this.workEffort = this.workEffortInventoryAssignment.Assignment;
          this.inventoryItemSelected(
            this.workEffortInventoryAssignment.InventoryItem
          );

          if (this.workEffortInventoryAssignment.canWriteInventoryItem) {
            this.title = 'Edit inventory assignment';
          } else {
            this.title = 'View inventory assignment';
          }
        }
      });
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
