import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  FixedAsset,
  RequirementState,
  WorkEffort,
  WorkRequirement,
  WorkRequirementFulfillment,
} from '@allors/workspace/domain/default';
import {
  ObjectData,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { And } from '@allors/system/workspace/domain';

@Component({
  templateUrl: './workrequirementfulfillment-create.component.html',
  providers: [ContextService],
})
export class WorkRequirementFulfillmentCreateComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  public title = 'Add Work Requirement Fulfillment';
  workRequirementFulfillment: WorkRequirementFulfillment;
  workEffort: WorkEffort;
  fixedAsset: FixedAsset;
  workRequirement: WorkRequirement;
  workRequirementsFilter: SearchFactory;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkRequirementFulfillmentCreateComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService
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
          const pulls = [
            pull.WorkEffort({
              objectId: this.data.associationId,
              include: {
                WorkEffortFixedAssetAssignmentsWhereAssignment: {
                  FixedAsset: x,
                },
              },
            }),
            pull.RequirementState({}),
          ];

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded })));
        })
      )
      .subscribe(({ loaded }) => {
        this.allors.context.reset();

        const requirementStates = loaded.collection<RequirementState>(
          m.RequirementState
        );
        const requirementCreated = requirementStates?.find(
          (v) => v.UniqueId === '7435eaa5-4739-4e48-8c6a-3e5645b69d9c'
        );

        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.fixedAsset =
          this.workEffort.WorkEffortFixedAssetAssignmentsWhereAssignment[0]?.FixedAsset;

        this.workRequirementFulfillment =
          this.allors.context.create<WorkRequirementFulfillment>(
            m.WorkRequirementFulfillment
          );
        this.workRequirementFulfillment.FullfillmentOf = this.workEffort;

        this.workRequirementsFilter = new SearchFactory({
          objectType: this.m.WorkRequirement,
          roleTypes: [this.m.WorkRequirement.Description],
          post: (predicate: And) => {
            predicate.operands.push(
              {
                kind: 'Equals',
                propertyType: m.WorkRequirement.FixedAsset,
                object: this.fixedAsset,
              },
              {
                kind: 'Equals',
                propertyType: m.WorkRequirement.RequirementState,
                object: requirementCreated,
              }
            );
          },
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workRequirementFulfillment);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
