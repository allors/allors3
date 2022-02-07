import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  WorkEffort,
  WorkTask,
  WorkEffortState,
  Priority,
  WorkEffortPurpose,
} from '@allors/default/workspace/domain';
import {
  NavigationService,
  OldPanelService,
  RefreshService,
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'worktask-overview-detail',
  templateUrl: './worktask-overview-detail.component.html',
  providers: [OldPanelService, ContextService],
})
export class WorkTaskOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  workTask: WorkTask;
  party: Party;
  workEffortStates: WorkEffortState[];
  priorities: Priority[];
  workEffortPurposes: WorkEffortPurpose[];
  employees: Person[];
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  addContactPerson = false;
  addContactMechanism: boolean;

  private subscription: Subscription;
  workEfforts: WorkEffort[];
  customersFilter: SearchFactory;
  subContractorsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: OldPanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'WorkTask Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Minimized
    const pullName = `${this.panel.name}_${this.m.WorkTask.tag}`;

    panel.onPull = (pulls) => {
      this.workTask = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};
        const id = this.panel.manager.id;

        pulls.push(
          pull.WorkTask({
            name: pullName,
            objectId: id,
            include: {
              WorkEffortState: x,
              FullfillContactMechanism: x,
              ContactPerson: x,
              CreatedBy: x,
              PublicElectronicDocuments: x,
              PrivateElectronicDocuments: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.workTask = loaded.object<WorkTask>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;

    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.workTask = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            pull.Organisation({
              objectId: this.internalOrganisationId.value,
              name: 'InternalOrganisation',
              include: {
                ActiveEmployees: {
                  CurrentPartyContactMechanisms: {
                    ContactMechanism: x,
                  },
                },
              },
            }),
            pull.WorkTask({
              objectId: id,
              include: {
                WorkEffortState: x,
                FullfillContactMechanism: x,
                Priority: x,
                WorkEffortPurposes: x,
                Customer: x,
                ExecutedBy: x,
                ContactPerson: x,
                CreatedBy: x,
              },
            }),
            pull.Locale({
              sorting: [{ roleType: m.Locale.Name }],
            }),
            pull.WorkEffortState({
              sorting: [{ roleType: m.WorkEffortState.Name }],
            }),
            pull.Priority({
              predicate: {
                kind: 'Equals',
                propertyType: m.Priority.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Priority.Name }],
            }),
            pull.WorkEffortPurpose({
              predicate: {
                kind: 'Equals',
                propertyType: this.m.WorkEffortPurpose.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.WorkEffortPurpose.Name }],
            }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            this.internalOrganisationId.value
          );
          this.subContractorsFilter = Filters.subContractorsFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const internalOrganisation = loaded.object<InternalOrganisation>(
          m.InternalOrganisation
        );
        this.employees = internalOrganisation.ActiveEmployees;

        this.workTask = loaded.object<WorkTask>(m.WorkTask);
        this.workEffortStates = loaded.collection<WorkEffortState>(
          m.WorkEffortState
        );
        this.priorities = loaded.collection<Priority>(m.Priority);
        this.workEffortPurposes = loaded.collection<WorkEffortPurpose>(
          m.WorkEffortPurpose
        );

        this.updateCustomer(this.workTask.Customer);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public contactPersonAdded(contact: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.workTask
      .Customer as Organisation;
    organisationContactRelationship.Contact = contact;

    this.contacts.push(contact);
    this.workTask.ContactPerson = contact;
  }

  public contactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.workTask.Customer.addPartyContactMechanism(partyContactMechanism);
    this.workTask.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public customerSelected(customer: IObject) {
    this.updateCustomer(customer as Party);
  }

  private updateCustomer(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: party,
        select: {
          PartyContactMechanisms: x,
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Party({
        object: party,
        select: {
          WorkEffortsWhereCustomer: {
            include: {
              WorkEffortState: x,
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.workEfforts = loaded.collection<WorkEffort>(
        m.Party.WorkEffortsWhereCustomer
      );
      const indexMyself = this.workEfforts.indexOf(this.workTask, 0);
      if (indexMyself > -1) {
        this.workEfforts.splice(indexMyself, 1);
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );

      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );

      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.errorService.errorHandler);
  }
}
