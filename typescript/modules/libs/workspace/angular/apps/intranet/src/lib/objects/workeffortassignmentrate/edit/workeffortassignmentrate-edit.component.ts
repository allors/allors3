import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { WorkEffort, WorkEffortPartyAssignment, WorkEffortAssignmentRate, TimeFrequency, RateType } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './workeffortassignmentrate-edit.component.html',
  providers: [ContextService],
})
export class WorkEffortAssignmentRateEditComponent implements OnInit, OnDestroy {
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
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortAssignmentRateEditComponent>,
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
          const isCreate = this.data.id == null;

          const pulls = [pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }), pull.TimeFrequency({ sorting: [{ roleType: this.m.TimeFrequency.Name }] })];

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

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);
        this.workEffortPartyAssignments = loaded.collection<WorkEffortPartyAssignment>(m.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment);
        this.rateTypes = loaded.collection<RateType>(m.RateType);
        this.timeFrequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        const hour = this.timeFrequencies?.find((v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5');

        if (isCreate) {
          this.title = 'Add Rate';
          this.workEffortAssignmentRate = this.allors.context.create<WorkEffortAssignmentRate>(m.WorkEffortAssignmentRate);
          this.workEffortAssignmentRate.WorkEffort = this.workEffort;
          this.workEffortAssignmentRate.Frequency = hour;
        } else {
          this.workEffortAssignmentRate = loaded.object<WorkEffortAssignmentRate>(m.WorkEffortAssignmentRate);

          if (this.workEffortAssignmentRate.canWriteRate) {
            this.title = 'Edit Rate';
          } else {
            this.title = 'View Rate';
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
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workEffortAssignmentRate);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
