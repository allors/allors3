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
  WorkEffortAssignmentRate,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
@Component({
  templateUrl: './workeffortassignmentrate-form.component.html',
  providers: [ContextService],
})
export class WorkEffortAssignmentRateFormComponent
  extends AllorsFormComponent<WorkEffortAssignmentRate>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  title: string;
  subTitle: string;

  readonly m: M;

  workEffortAssignmentRate: WorkEffortAssignmentRate;
  workEffort: WorkEffort;
  workEffortPartyAssignments: WorkEffortPartyAssignment[];
  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
            pull.TimeFrequency({
              sorting: [{ roleType: this.m.TimeFrequency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortAssignmentRate({
                objectId: this.data.id,
                include: {
                  RateType: x,
                  Frequency: x,
                  WorkEffortPartyAssignment: x,
                  WorkEffort: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.WorkEffort({
                objectId: this.data.associationId,
                select: {
                  WorkEffortPartyAssignmentsWhereAssignment: {
                    include: {
                      Party: x,
                    },
                  },
                },
              }),
              pull.WorkEffort({
                objectId: this.data.associationId,
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.workEffortPartyAssignments =
          loaded.collection<WorkEffortPartyAssignment>(
            m.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment
          );
        this.rateTypes = loaded.collection<RateType>(m.RateType);
        this.timeFrequencies = loaded.collection<TimeFrequency>(
          m.TimeFrequency
        );
        const hour = this.timeFrequencies?.find(
          (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
        );

        if (isCreate) {
          this.title = 'Add Rate';
          this.workEffortAssignmentRate =
            this.allors.context.create<WorkEffortAssignmentRate>(
              m.WorkEffortAssignmentRate
            );
          this.workEffortAssignmentRate.WorkEffort = this.workEffort;
          this.workEffortAssignmentRate.Frequency = hour;
        } else {
          this.workEffortAssignmentRate =
            loaded.object<WorkEffortAssignmentRate>(m.WorkEffortAssignmentRate);

          if (this.workEffortAssignmentRate.canWriteRate) {
            this.title = 'Edit Rate';
          } else {
            this.title = 'View Rate';
          }
        }
      });
  }
}
