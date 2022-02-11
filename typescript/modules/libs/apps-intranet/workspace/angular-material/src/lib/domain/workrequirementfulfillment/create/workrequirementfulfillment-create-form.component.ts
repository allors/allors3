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
  WorkRequirementFulfillment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './workrequirementfulfillment-create-form.component.html',
  providers: [ContextService],
})
export class WorkRequirementFulfillmentCreateFormComponent
  extends AllorsFormComponent<WorkRequirementFulfillment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
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
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.internalOrganisation);

    if (this.editRequest) {
      pulls.push(
        p.BasePrice({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Currency: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);

    this.object.FromDate = new Date();
    this.object.PricedBy = this.internalOrganisation;
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
}
