import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, And } from '@allors/system/workspace/domain';
import {
  FixedAsset,
  RequirementState,
  WorkEffort,
  WorkRequirement,
  WorkRequirementFulfillment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './workrequirementfulfillment-create-form.component.html',
  providers: [ContextService],
})
export class WorkRequirementFulfillmentCreateFormComponent extends AllorsFormComponent<WorkRequirementFulfillment> {
  readonly m: M;
  workEffort: WorkEffort;
  fixedAsset: FixedAsset;
  workRequirement: WorkRequirement;
  workRequirementsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(p.RequirementState({}));

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffort({
          objectId: initializer.id,
          include: {
            WorkEffortFixedAssetAssignmentsWhereAssignment: {
              FixedAsset: {},
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    const requirementStates = pullResult.collection<RequirementState>(
      this.m.RequirementState
    );
    const requirementCreated = requirementStates?.find(
      (v) => v.UniqueId === '7435eaa5-4739-4e48-8c6a-3e5645b69d9c'
    );

    this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
    this.fixedAsset =
      this.workEffort.WorkEffortFixedAssetAssignmentsWhereAssignment[0]?.FixedAsset;

    this.object.FullfillmentOf = this.workEffort;

    this.workRequirementsFilter = new SearchFactory({
      objectType: this.m.WorkRequirement,
      roleTypes: [this.m.WorkRequirement.Description],
      post: (predicate: And) => {
        predicate.operands.push(
          {
            kind: 'Equals',
            propertyType: this.m.WorkRequirement.FixedAsset,
            object: this.fixedAsset,
          },
          {
            kind: 'Equals',
            propertyType: this.m.WorkRequirement.RequirementState,
            object: requirementCreated,
          }
        );
      },
    });
  }
}
