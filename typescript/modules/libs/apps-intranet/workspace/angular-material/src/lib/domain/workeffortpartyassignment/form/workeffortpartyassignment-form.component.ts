import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Employment,
  Party,
  Person,
  WorkEffort,
  WorkEffortPartyAssignment,
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
export class WorkEffortPartyAssignmentFormComponent extends AllorsFormComponent<WorkEffortPartyAssignment> {
  readonly m: M;
  people: Person[];
  person: Person;
  party: Party;
  workEffort: WorkEffort;
  assignment: WorkEffort;
  contacts: Person[] = [];
  employees: Person[];
  employments: Employment[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Organisation({
        objectId: this.internalOrganisationId.value,
        select: {
          EmploymentsWhereEmployer: {
            include: {
              Employee: {},
            },
          },
        },
        sorting: [{ roleType: m.Person.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortPartyAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            Party: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
        }),
        p.WorkEffort({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    if (this.createRequest) {
      this.party = pullResult.object<Party>(this.m.Party);
      this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
      this.employments = pullResult.collection<Employment>(
        this.m.Organisation.EmploymentsWhereEmployer
      );

      if (this.party != null && this.party.strategy.cls === this.m.Person) {
        this.person = this.party as Person;
        this.object.Party = this.person;
      }

      if (
        this.workEffort != null &&
        this.workEffort.strategy.cls === this.m.WorkTask
      ) {
        this.assignment = this.workEffort as WorkEffort;
        this.object.Assignment = this.assignment;
      }
    } else {
      this.party = this.object.Party;
      this.workEffort = this.object.Assignment;
      this.person = this.object.Party as Person;
      this.assignment = this.object.Assignment;
    }

    this.fromDateSelected();
  }

  public fromDateSelected(): void {
    if (this.workEffort) {
      const fromDate = this.object.FromDate ?? new Date();
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
}
