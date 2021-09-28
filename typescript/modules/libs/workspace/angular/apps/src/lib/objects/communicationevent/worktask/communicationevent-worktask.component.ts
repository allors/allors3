import { Component, Self, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  WorkTask,
  Good,
  InternalOrganisation,
  NonUnifiedGood,
  Part,
  PriceComponent,
  Brand,
  Model,
  Locale,
  Carrier,
  SerialisedItemCharacteristicType,
  WorkTask,
  ContactMechanism,
  Person,
  Organisation,
  PartyContactMechanism,
  OrganisationContactRelationship,
  Catalogue,
  Singleton,
  ProductCategory,
  Scope,
  CommunicationEvent,
  WorkEffortState,
  Priority,
  WorkEffortPurpose,
  WorkEffortPartyAssignment,
} from '@allors/workspace/domain/default';
import {
  Action,
  DeleteService,
  EditService,
  Filter,
  FilterDefinition,
  MediaService,
  NavigationService,
  ObjectData,
  ObjectService,
  OverviewService,
  PanelService,
  RefreshService,
  SaveService,
  SearchFactory,
  Sorter,
  Table,
  TableRow,
  TestScope,
} from '@allors/workspace/angular/base';
import { SessionService, WorkspaceService } from '@allors/workspace/angular/core';
import { And } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './communicationevent-worktask.component.html',
  providers: [SessionService],
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

  constructor(
    @Self() public allors: SessionService,

    private saveService: SaveService,
    private route: ActivatedRoute,
    public refreshService: RefreshService,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    titleService.setTitle(this.title);

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
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
              include: { CommunicationEventState: x },
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

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.subTitle = 'edit work task';
        this.workTask = loaded.object<Worktask>(m.Worktask);
        const communicationEvent: CommunicationEvent = loaded.object<CommunicationEvent>(m.CommunicationEvent);

        if (!this.workTask) {
          this.subTitle = 'add a new work task';
          this.workTask = this.allors.session.create<WorkTask>(m.WorkTask);
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
      const workEffortPartyAssignment: WorkEffortPartyAssignment = this.allors.session.create<WorkEffortPartyAssignment>(m.WorkEffortPartyAssignment);
      workEffortPartyAssignment.Assignment = this.workTask;
      workEffortPartyAssignment.Party = assignee;
    });

    this.allors.client.pushReactive(this.allors.session).subscribe((saved: Saved) => {
      this.goBack();
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public goBack(): void {
    window.history.back();
  }
}
