import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Person,
  Party,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  WorkTask,
} from '@allors/workspace/domain/default';
import {
  NavigationService,
  RefreshService,
  SaveService,
  SearchFactory,
  UserId,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  templateUrl: './worktask-create.component.html',
  providers: [ContextService],
})
export class WorkTaskCreateComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Add Work Task';

  add: boolean;
  edit: boolean;

  internalOrganisation: InternalOrganisation;
  workTask: WorkTask;
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  addContactPerson = false;
  addContactMechanism: boolean;

  private subscription: Subscription;
  private readonly refresh$: BehaviorSubject<Date>;
  organisationsFilter: SearchFactory;
  subContractorsFilter: SearchFactory;
  user: Person;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<WorkTaskCreateComponent>,
    public navigationService: NavigationService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private route: ActivatedRoute,
    private userId: UserId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.route.url, this.refresh$])
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Person({
              objectId: this.userId.value,
              include: {
                CurrentOrganisationContactRelationships: {
                  Organisation: x,
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.user = loaded.object<Person>(m.Person);
        this.workTask = this.allors.context.create<WorkTask>(m.WorkTask);

        if (this.user.CurrentOrganisationContactRelationships.length == 1) {
          const customer =
            this.user.CurrentOrganisationContactRelationships[0].Organisation;
          this.updateCustomer(customer as Party);
          this.workTask.Customer = customer;
        }
      });
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
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.workTask);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
