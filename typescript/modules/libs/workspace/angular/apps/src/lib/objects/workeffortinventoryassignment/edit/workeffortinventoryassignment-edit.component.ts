import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { Organisation, InventoryItem, Part, Facility, SerialisedInventoryItem, NonSerialisedInventoryItemState, SerialisedInventoryItemState, NonSerialisedInventoryItem, WorkEffort, WorkEffortInventoryAssignment } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { InternalOrganisationId } from '@allors/angular/base';
import { IObject, ISessionObject } from '@allors/domain/system';
import { Sort } from '@allors/data/system';
import { TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './workeffortinventoryassignment-edit.component.html',
  providers: [SessionService]
})
export class WorkEffortInventoryAssignmentEditComponent extends TestScope implements OnInit, OnDestroy {

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
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortInventoryAssignmentEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    private snackBar: MatSnackBar
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          let pulls = [
            pull.InventoryItem({
              sorting: [{ roleType: m.InventoryItem.Name }],
              include: {
                Part: {
                  InventoryItemKind: x
                },
                Facility: x,
                SerialisedInventoryItem_SerialisedInventoryItemState: x,
                NonSerialisedInventoryItem_NonSerialisedInventoryItemState: x
              }
            }),
            pull.Organisation({
              object: this.internalOrganisationId.value,
              name: 'InternalOrganisation',
              include: {
                FacilitiesWhereOwner: x,
              }
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
                      InventoryItemKind: x
                    },
                  }
                }
              }),
            );
          }

          if (isCreate && this.data.associationId) {
            pulls = [
              ...pulls,
              pull.WorkEffort({
                object: this.data.associationId
              }),
            ];
          }

          return this.allors.context
            .load(new PullRequest({ pulls }))
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {

        this.allors.session.reset();

        const internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);

        const inventoryItems = loaded.collection<InventoryItem>(m.InventoryItem);
        this.inventoryItems = inventoryItems.filter(v => internalOrganisation.FacilitiesWhereOwner.includes(v.Facility));

        if (isCreate) {
          this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);

          this.title = 'Add inventory assignment';

          this.workEffortInventoryAssignment = this.allors.session.create<WorkEffortInventoryAssignment>(m.WorkEffortInventoryAssignment);
          this.workEffortInventoryAssignment.Assignment = this.workEffort;

        } else {
          this.workEffortInventoryAssignment = loaded.object<WorkEffortInventoryAssignment>(m.WorkEffortInventoryAssignment);
          this.workEffort = this.workEffortInventoryAssignment.Assignment;
          this.inventoryItemSelected(this.workEffortInventoryAssignment.InventoryItem);

          if (this.workEffortInventoryAssignment.canWriteInventoryItem) {
            this.title = 'Edit inventory assignment';
          } else {
            this.title = 'View inventory assignment';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public update(): void {
    const { context } = this.allors;

    context
      .save()
      .subscribe(() => {
        this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }

  public save(): void {

    this.allors.client.pushReactive(this.allors.session)
      .subscribe(() => {
        const data: IObject = {
          id: this.workEffortInventoryAssignment.id,
          objectType: this.workEffortInventoryAssignment.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }

  public inventoryItemSelected(inventoryItem: ISessionObject): void {
    this.serialised = (inventoryItem as InventoryItem).Part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

    if (inventoryItem.objectType === this.metaService.m.NonSerialisedInventoryItem) {
      const item = inventoryItem as NonSerialisedInventoryItem;
      this.state = item.NonSerialisedInventoryItemState;
    } else {
      const item = inventoryItem as SerialisedInventoryItem;
      this.state = item.SerialisedInventoryItemState;
    }
  }
}
