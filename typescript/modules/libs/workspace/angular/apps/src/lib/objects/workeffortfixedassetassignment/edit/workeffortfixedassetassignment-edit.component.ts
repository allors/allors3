import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Party, SerialisedItem, WorkEffort, WorkEffortFixedAssetAssignment, Enumeration } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './workeffortfixedassetassignment-edit.component.html',
  providers: [SessionService],
})
export class WorkEffortFixedAssetAssignmentEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  workEffortFixedAssetAssignment: WorkEffortFixedAssetAssignment;
  workEffort: WorkEffort;
  assignment: WorkEffort;
  serialisedItem: SerialisedItem;
  assetAssignmentStatuses: Enumeration[];
  title: string;

  private subscription: Subscription;
  serialisedItems: SerialisedItem[];
  externalCustomer: boolean;

  serialisedItemsFilter: SearchFactory;
  workEfforts: WorkEffort[];

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortFixedAssetAssignmentEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.WorkEffort({
              sorting: [{ roleType: m.WorkEffort.Name }],
            }),
            pull.SerialisedItem({
              objectId: this.data.associationId,
              sorting: [{ roleType: m.SerialisedItem.Name }],
            }),
            pull.AssetAssignmentStatus({
              predicate: { kind: 'Equals', propertyType: m.AssetAssignmentStatus.IsActive, value: true },
              sorting: [{ roleType: m.AssetAssignmentStatus.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortFixedAssetAssignment({
                objectId: this.data.id,
                include: {
                  Assignment: x,
                  FixedAsset: x,
                  AssetAssignmentStatus: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.WorkEffort({
                objectId: this.data.associationId,
              })
            );
          }

          this.serialisedItemsFilter = Filters.serialisedItemsFilter(m);

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.workEfforts = loaded.collection<WorkEffort>(m.WorkEffort);
        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        this.assetAssignmentStatuses = loaded.collection<Enumeration>(m.Enumeration);

        if (this.serialisedItem === undefined) {
          const b2bCustomer = this.workEffort.Customer as Organisation;
          this.externalCustomer = b2bCustomer === null || !b2bCustomer.IsInternalOrganisation;

          if (this.externalCustomer) {
            this.updateSerialisedItems(this.workEffort.Customer);
          }
        }

        if (isCreate) {
          this.title = 'Add Asset Assignment';

          this.workEffortFixedAssetAssignment = this.allors.session.create<WorkEffortFixedAssetAssignment>(m.WorkEffortFixedAssetAssignment);

          if (this.serialisedItem !== undefined) {
            this.workEffortFixedAssetAssignment.FixedAsset = this.serialisedItem;
          }

          if (this.workEffort !== undefined && this.workEffort.strategy.cls === m.WorkTask) {
            this.assignment = this.workEffort as WorkEffort;
            this.workEffortFixedAssetAssignment.Assignment = this.assignment;
          }
        } else {
          this.workEffortFixedAssetAssignment = loaded.object<WorkEffortFixedAssetAssignment>(m.WorkEffortFixedAssetAssignment);

          if (this.workEffortFixedAssetAssignment.canWriteFromDate) {
            this.title = 'Edit Asset Assignment';
          } else {
            this.title = 'View Asset Assignment';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.dialogRef.close(this.workEffortFixedAssetAssignment);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
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

    this.allors.client.pullReactive(this.allors.session, pulls).subscribe((loaded) => {
      this.serialisedItems = loaded.collection<SerialisedItem>(m.SerialisedItem);
    });
  }
}
