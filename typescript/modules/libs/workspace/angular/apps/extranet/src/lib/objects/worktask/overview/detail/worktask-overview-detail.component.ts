import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Party, ContactMechanism, PartyContactMechanism, WorkEffort, WorkTask } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'worktask-overview-detail',
  templateUrl: './worktask-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class WorkTaskOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  workTask: WorkTask;
  party: Party;
  contactMechanisms: ContactMechanism[];
  contacts: Person[];

  private subscription: Subscription;
  workEfforts: WorkEffort[];

  constructor(@Self() public allors: ContextService, @Self() public panel: PanelService, public refreshService: RefreshService, public navigationService: NavigationService, private saveService: SaveService) {
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
              CreatedBy: x,
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
            pull.WorkTask({
              objectId: id,
              include: {
                WorkEffortState: x,
                FullfillContactMechanism: x,
                Priority: x,
                WorkEffortPurposes: x,
                ExecutedBy: x,
                ContactPerson: x,
                CreatedBy: x,
                Customer: x,
                PublicElectronicDocuments: x,
              },
            }),
            pull.Locale({
              sorting: [{ roleType: m.Locale.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.workTask = loaded.object<WorkTask>(m.WorkTask);

        this.updateCustomer(this.workTask.Customer);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
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
      this.workEfforts = loaded.collection<WorkEffort>(m.Party.WorkEffortsWhereCustomer);
      const indexMyself = this.workEfforts.indexOf(this.workTask, 0);
      if (indexMyself > -1) {
        this.workEfforts.splice(indexMyself, 1);
      }

      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);

      this.contactMechanisms = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);

      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
