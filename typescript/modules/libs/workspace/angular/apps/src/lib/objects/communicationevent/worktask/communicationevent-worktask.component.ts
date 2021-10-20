import { Component, Self, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, WorkTask, Person, CommunicationEvent, WorkEffortState, Priority, WorkEffortPurpose, WorkEffortPartyAssignment } from '@allors/workspace/domain/default';
import { RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './communicationevent-worktask.component.html',
  providers: [ContextService],
})
export class CommunicationEventWorkTaskComponent implements OnInit, OnDestroy {
  public title = 'Work Task';
  public subTitle: string;

  public m: M;

  public workTask: WorkTask;

  public workEffortStates: WorkEffortState[];
  public priorities: Priority[];
  public workEffortPurposes: WorkEffortPurpose[];
  public employees: Person[];
  public workEffortPartyAssignments: WorkEffortPartyAssignment[];
  public assignees: Person[] = [];

  private subscription: Subscription;

  constructor(@Self() public allors: ContextService, private saveService: SaveService, private route: ActivatedRoute, public refreshService: RefreshService, private internalOrganisationId: InternalOrganisationId, titleService: Title) {
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.route.url, this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(([urlSegments, date, internalOrganisationId]) => {
          const id: string = this.route.snapshot.paramMap.get('id');
          const roleId: string = this.route.snapshot.paramMap.get('roleId');

          const pulls = [
            pull.CommunicationEvent({
              objectId: id,
              include: { 
                WorkEfforts: x,
                CommunicationEventState: x,
              },
            }),
            pull.WorkTask({
              objectId: roleId,
            }),
            pull.InternalOrganisation({
              objectId: id,
              include: { ActiveEmployees: x },
            }),
            pull.WorkEffortState({
              sorting: [{ roleType: m.WorkEffortState.Name }],
            }),
            pull.Priority({
              predicate: { kind: 'Equals', propertyType: m.Priority.IsActive, value: true },
              sorting: [{ roleType: m.Priority.Name }],
            }),
            pull.WorkEffortPurpose({
              predicate: { kind: 'Equals', propertyType: m.WorkEffortPurpose.IsActive, value: true },
              sorting: [{ roleType: m.WorkEffortPurpose.Name }],
            }),
            pull.WorkEffortPartyAssignment({}),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.subTitle = 'edit work task';
        this.workTask = loaded.object<WorkTask>(m.WorkTask);
        const communicationEvent: CommunicationEvent = loaded.object<CommunicationEvent>(m.CommunicationEvent);

        if (!this.workTask) {
          this.subTitle = 'add a new work task';
          this.workTask = this.allors.context.create<WorkTask>(m.WorkTask);
          communicationEvent.addWorkEffort(this.workTask);
        }

        this.workEffortStates = loaded.collection<WorkEffortState>(m.WorkEffortState);
        this.priorities = loaded.collection<Priority>(m.Priority);
        this.workEffortPurposes = loaded.collection<WorkEffortPurpose>(m.WorkEffortPurpose);
        const internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.employees = internalOrganisation.ActiveEmployees;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.assignees.forEach((assignee: Person) => {
      const workEffortPartyAssignment: WorkEffortPartyAssignment = this.allors.context.create<WorkEffortPartyAssignment>(this.m.WorkEffortPartyAssignment);
      workEffortPartyAssignment.Assignment = this.workTask;
      workEffortPartyAssignment.Party = assignee;
    });

    this.allors.context.push().subscribe(() => {
      this.goBack();
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public goBack(): void {
    window.history.back();
  }
}
