import { Component, Self } from '@angular/core';

import {
  Person,
  ContactMechanism,
  WorkTask,
} from '@allors/default/workspace/domain';
import {
  ErrorService,
  AllorsFormComponent,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  IObject,
  IPullResult,
  Node,
  Pull,
} from '@allors/system/workspace/domain';
import { NgForm } from '@angular/forms';
import { M } from '@allors/default/workspace/meta';

@Component({
  selector: 'worktask-edit-form',
  templateUrl: './worktask-edit-form.component.html',
  providers: [ContextService],
})
export class WorkTaskEditFormComponent extends AllorsFormComponent<WorkTask> {
  m: M;

  contactMechanisms: ContactMechanism[];
  contacts: Person[];

  constructor(
    @Self() private allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onEditInclude(): Node[] {
    // TODO: KOEN
    const { treeBuilder: t } = this.m;

    return t.BasePrice({
      Currency: {},
    });
  }

  onPreEditPull(objectId: number, pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Organisation({
        predicate: {
          kind: 'ContainedIn',
          propertyType: m.Organisation.CurrentOrganisationContactRelationships,
          extent: {
            kind: 'Filter',
            objectType: m.OrganisationContactRelationship,
            predicate: {
              kind: 'Equals',
              propertyType: m.OrganisationContactRelationship.Contact,
              value: this.userId.value,
            },
          },
        },
        results: [
          {
            name: 'contactmechanisms',
            select: {
              PartyContactMechanisms: {},
              CurrentPartyContactMechanisms: {
                include: {
                  ContactMechanism: {
                    PostalAddress_Country: {},
                  },
                },
              },
            },
          },
          {
            name: 'contacts',
            select: {
              CurrentContacts: {},
            },
          },
        ],
      })
    );
  }

  onPostEditPull(object: IObject, loaded: IPullResult): void {
    this.contactMechanisms =
      loaded.collection<ContactMechanism>('contactmechanisms');
    this.contacts = loaded.collection<Person>('contacts');
  }

  // constructor(
  //   @Self() public allors: ContextService,
  //   @Self() public panel: OldPanelService,
  //   public refreshService: RefreshService,
  //   public navigationService: NavigationService,
  //   private errorService: ErrorService
  // ) {
  //   this.allors.context.name = this.constructor.name;
  //   this.m = this.allors.context.configuration.metaPopulation as M;

  //   panel.name = 'detail';
  //   panel.title = 'WorkTask Details';
  //   panel.icon = 'business';
  //   panel.expandable = true;

  //   // Minimized
  //   const pullName = `${this.panel.name}_${this.m.WorkTask.tag}`;

  //   panel.onPull = (pulls) => {
  //     this.object = undefined;

  //     if (this.panel.isCollapsed) {
  //       const m = this.m;
  //       const { pullBuilder: pull } = m;
  //       const x = {};
  //       const id = this.panel.manager.id;

  //       pulls.push(
  //         pull.WorkTask({
  //           name: pullName,
  //           objectId: id,
  //           include: {
  //             WorkEffortState: x,
  //             CreatedBy: x,
  //           },
  //         })
  //       );
  //     }
  //   };

  //   panel.onPulled = (loaded) => {
  //     if (this.panel.isCollapsed) {
  //       this.object = loaded.object<WorkTask>(pullName);
  //     }
  //   };
  // }

  // public ngOnInit(): void {
  //   const m = this.m;

  //   // Maximized
  //   this.subscription = this.panel.manager.on$
  //     .pipe(
  //       filter(() => {
  //         return this.panel.isExpanded;
  //       }),
  //       switchMap(() => {
  //         this.object = undefined;

  //         const m = this.m;
  //         const { pullBuilder: pull } = m;
  //         const x = {};
  //         const id = this.panel.manager.id;

  //         const pulls = [
  //           pull.WorkTask({
  //             objectId: id,
  //             include: {
  //               WorkEffortState: x,
  //               FullfillContactMechanism: x,
  //               Priority: x,
  //               WorkEffortPurposes: x,
  //               ExecutedBy: x,
  //               ContactPerson: x,
  //               CreatedBy: x,
  //               Customer: x,
  //               PublicElectronicDocuments: x,
  //             },
  //           }),
  //           pull.Locale({
  //             sorting: [{ roleType: m.Locale.Name }],
  //           }),
  //         ];

  //         return this.allors.context.pull(pulls);
  //       })
  //     )
  //     .subscribe((loaded) => {
  //       this.allors.context.reset();

  //       this.object = loaded.object<WorkTask>(m.WorkTask);

  //       this.updateCustomer(this.object.Customer);
  //     });
  // }

  // public ngOnDestroy(): void {
  //   if (this.subscription) {
  //     this.subscription.unsubscribe();
  //   }
  // }

  // private updateCustomer(party: Party) {
  //   const m = this.m;
  //   const { pullBuilder: pull } = m;
  //   const x = {};

  //   const pulls = [
  //     pull.Party({
  //       object: party,
  //       select: {
  //         PartyContactMechanisms: x,
  //         CurrentPartyContactMechanisms: {
  //           include: {
  //             ContactMechanism: {
  //               PostalAddress_Country: x,
  //             },
  //           },
  //         },
  //       },
  //     }),
  //     pull.Party({
  //       object: party,
  //       select: {
  //         CurrentContacts: x,
  //       },
  //     }),
  //     pull.Party({
  //       object: party,
  //       select: {
  //         WorkEffortsWhereCustomer: {
  //           include: {
  //             WorkEffortState: x,
  //           },
  //         },
  //       },
  //     }),
  //   ];

  //   this.allors.context.pull(pulls).subscribe((loaded) => {
  //     this.workEfforts = loaded.collection<WorkEffort>(
  //       m.Party.WorkEffortsWhereCustomer
  //     );
  //     const indexMyself = this.workEfforts.indexOf(this.object, 0);
  //     if (indexMyself > -1) {
  //       this.workEfforts.splice(indexMyself, 1);
  //     }

  //     const partyContactMechanisms: PartyContactMechanism[] =
  //       loaded.collection<PartyContactMechanism>(
  //         m.Party.CurrentPartyContactMechanisms
  //       );

  //     this.contactMechanisms = partyContactMechanisms?.map(
  //       (v: PartyContactMechanism) => v.ContactMechanism
  //     );

  //     this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
  //   });
  // }

  // public save(): void {
  //   this.allors.context.push().subscribe(() => {
  //     this.refreshService.refresh();
  //     this.panel.toggle();
  //   }, this.errorService.errorHandler);
  // }
}
