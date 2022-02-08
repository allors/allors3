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
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './workeffortpartyassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortPartyAssignmentFormComponent
  implements OnInit, OnDestroy
{
  readonly m: M;

  workEffortPartyAssignment: WorkEffortPartyAssignment;
  people: Person[];
  person: Person;
  party: Party;
  workEffort: WorkEffort;
  assignment: WorkEffort;
  contacts: Person[] = [];
  title: string;

  private subscription: Subscription;
  employees: Person[];
  employments: Employment[];

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkEffortPartyAssignmentFormComponent>,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id == null;

          let pulls = [
            pull.Organisation({
              objectId: internalOrganisationId,
              select: {
                EmploymentsWhereEmployer: {
                  include: {
                    Employee: x,
                  },
                },
              },
              sorting: [{ roleType: m.Person.DisplayName }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.WorkEffortPartyAssignment({
                objectId: this.data.id,
                include: {
                  Assignment: x,
                  Party: x,
                },
              })
            );
          }

          if (isCreate) {
            pulls = [
              ...pulls,
              pull.Party({
                objectId: this.data.associationId,
              }),
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

        if (isCreate) {
          this.title = 'Add Party Assignment';

          this.workEffortPartyAssignment =
            this.allors.context.create<WorkEffortPartyAssignment>(
              m.WorkEffortPartyAssignment
            );
          this.party = loaded.object<Party>(m.Party);
          this.workEffort = loaded.object<WorkEffort>(m.WorkEffort);

          if (this.party != null && this.party.strategy.cls === m.Person) {
            this.person = this.party as Person;
            this.workEffortPartyAssignment.Party = this.person;
          }

          if (
            this.workEffort != null &&
            this.workEffort.strategy.cls === m.WorkTask
          ) {
            this.assignment = this.workEffort as WorkEffort;
            this.workEffortPartyAssignment.Assignment = this.assignment;
          }
        } else {
          this.workEffortPartyAssignment =
            loaded.object<WorkEffortPartyAssignment>(
              m.WorkEffortPartyAssignment
            );
          this.party = this.workEffortPartyAssignment.Party;
          this.workEffort = this.workEffortPartyAssignment.Assignment;
          this.person = this.workEffortPartyAssignment.Party as Person;
          this.assignment = this.workEffortPartyAssignment.Assignment;

          if (this.workEffortPartyAssignment.canWriteFromDate) {
            this.title = 'Edit Party Assignment';
          } else {
            this.title = 'View Party Assignment';
          }
        }

        this.employments = loaded.collection<Employment>(
          m.Organisation.EmploymentsWhereEmployer
        );
        this.fromDateSelected();
      });
  }

  public fromDateSelected(): void {
    if (this.workEffort) {
      const fromDate = this.workEffortPartyAssignment.FromDate ?? new Date();
      this.employees = this.employments
        ?.filter(
          (v) =>
            v.FromDate <= fromDate &&
            (v.ThroughDate == null || v.ThroughDate >= fromDate)
        )
        ?.map((v) => v.Employee);
    } else {
      this.employees = [this.person];
    }
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workEffortPartyAssignment);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
