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
  TimeEntry,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './timeentry-form.component.html',
  providers: [ContextService],
})
export class TimeEntryFormComponent
  extends AllorsFormComponent<TimeEntry>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  title: string;
  subTitle: string;

  readonly m: M;

  frequencies: TimeFrequency[];

  private subscription: Subscription;
  timeEntry: TimeEntry;
  timeSheet: TimeSheet;
  workers: Party[];
  selectedWorker: Party;
  workEffort: WorkEffort;
  rateTypes: RateType[];
  workEffortAssignmentRates: WorkEffortAssignmentRate[];
  workEffortRate: WorkEffortAssignmentRate;
  partyRate: PartyRate;
  derivedBillingRate: string;
  customerRate: PartyRate;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private snackBar: MatSnackBar
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const workEffortPartyAssignmentPullName = `${this.m.WorkEffortPartyAssignment.tag}`;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          let pulls = [
            pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
            pull.TimeFrequency({
              sorting: [{ roleType: this.m.TimeFrequency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.TimeEntry({
                objectId: this.data.id,
                include: {
                  WorkEffort: x,
                  TimeFrequency: x,
                  BillingFrequency: x,
                },
              }),
              pull.TimeEntry({
                objectId: this.data.id,
                select: {
                  WorkEffort: {
                    WorkEffortPartyAssignmentsWhereAssignment: {
                      include: {
                        Party: x,
                      },
                    },
                  },
                },
              })
            );
          }

          if (isCreate) {
            pulls = [
              ...pulls,
              pull.WorkEffort({
                objectId: this.data.associationId,
              }),
              pull.WorkEffort({
                name: workEffortPartyAssignmentPullName,
                objectId: this.data.associationId,
                select: {
                  WorkEffortPartyAssignmentsWhereAssignment: {
                    include: {
                      Party: x,
                    },
                  },
                },
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

        this.rateTypes = loaded.collection<RateType>(m.RateType);
        this.frequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        const hour = this.frequencies?.find(
          (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
        );

        if (isCreate) {
          this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);

          this.title = 'Add Time Entry';
          this.timeEntry = this.allors.context.create<TimeEntry>(m.TimeEntry);
          this.timeEntry.WorkEffort = this.workEffort;
          this.timeEntry.IsBillable = true;
          this.timeEntry.BillingFrequency = hour;
          this.timeEntry.TimeFrequency = hour;

          const workEffortPartyAssignments =
            loaded.collection<WorkEffortPartyAssignment>(
              workEffortPartyAssignmentPullName
            );
          this.workers = Array.from(
            new Set(workEffortPartyAssignments?.map((v) => v.Party)).values()
          );
        } else {
          this.timeEntry = loaded.object<TimeEntry>(m.TimeEntry);
          this.selectedWorker = this.timeEntry.Worker;
          this.workEffort = this.timeEntry.WorkEffort;

          const workEffortPartyAssignments =
            loaded.collection<WorkEffortPartyAssignment>(
              m.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment
            );
          this.workers = Array.from(
            new Set(workEffortPartyAssignments?.map((v) => v.Party)).values()
          );

          if (this.timeEntry.canWriteAssignedAmountOfTime) {
            this.title = 'Edit Time Entry';
          } else {
            this.title = 'View Time Entry';
          }
        }

        if (!isCreate) {
          this.workerSelected(this.selectedWorker);
        }
      });
  }

  public findBillingRate(): void {
    if (
      this.selectedWorker &&
      this.timeEntry.RateType &&
      this.timeEntry.FromDate
    ) {
      this.workerSelected(this.selectedWorker);
    }
  }

  public workerSelected(party: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        objectId: party.id,
        select: {
          Person_TimeSheetWhereWorker: {
            include: { TimeEntries: x },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.timeSheet = loaded.object<TimeSheet>(m.Person.TimeSheetWhereWorker);
    });
  }

  public update(): void {
    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  public save(): void {
    if (!this.timeEntry.TimeSheetWhereTimeEntry) {
      this.timeSheet.addTimeEntry(this.timeEntry);
    }

    super.save();
  }
}
