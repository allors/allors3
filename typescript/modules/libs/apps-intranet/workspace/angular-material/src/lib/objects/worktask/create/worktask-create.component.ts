import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Locale,
  Person,
  Organisation,
  OrganisationContactRelationship,
  Party,
  InternalOrganisation,
  ContactMechanism,
  PartyContactMechanism,
  WorkTask,
  SerialisedItem,
  WorkEffortFixedAssetAssignment,
} from '@allors/workspace/domain/default';
import { NavigationService, ObjectData, RefreshService, SaveService, SearchFactory } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

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

  locales: Locale[];

  private subscription: Subscription;
  private readonly refresh$: BehaviorSubject<Date>;
  organisationsFilter: SearchFactory;
  subContractorsFilter: SearchFactory;
  workEffortFixedAssetAssignment: WorkEffortFixedAssetAssignment;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<WorkTaskCreateComponent>,
    public navigationService: NavigationService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private route: ActivatedRoute,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.route.url, this.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Locale({
              sorting: [{ roleType: m.Locale.Name }],
            }),
          ];

          if (this.data.associationId) {
            pulls.push(
              pull.SerialisedItem({
                objectId: this.data.associationId,
              }),
              pull.Party({
                objectId: this.data.associationId,
              })
            );
          }

          this.organisationsFilter = Filters.organisationsFilter(m);
          this.subContractorsFilter = Filters.subContractorsFilter(m, this.internalOrganisationId.value);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.locales = loaded.collection<Locale>(m.Locale);

        const fromSerialiseditem = loaded.object<SerialisedItem>(m.SerialisedItem);
        const fromCustomer = loaded.object<Party>(m.Party);

        this.workTask = this.allors.context.create<WorkTask>(m.WorkTask);
        this.workTask.TakenBy = this.internalOrganisation as Organisation;
        this.workTask.Customer = fromCustomer;

        if (fromSerialiseditem != null) {
          this.workEffortFixedAssetAssignment = this.allors.context.create<WorkEffortFixedAssetAssignment>(m.WorkEffortFixedAssetAssignment);
          this.workEffortFixedAssetAssignment.Assignment = this.workTask;
          this.workEffortFixedAssetAssignment.FixedAsset = fromSerialiseditem;
        }
      });
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
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const partyContactMechanisms: PartyContactMechanism[] = loaded.collection<PartyContactMechanism>(m.Party.CurrentPartyContactMechanisms);
      this.contactMechanisms = partyContactMechanisms?.map((v: PartyContactMechanism) => v.ContactMechanism);

      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  public contactPersonAdded(contact: Person): void {
    const organisationContactRelationship = this.allors.context.create<OrganisationContactRelationship>(this.m.OrganisationContactRelationship);
    organisationContactRelationship.Organisation = this.workTask.Customer as Organisation;
    organisationContactRelationship.Contact = contact;

    this.contacts.push(contact);
    this.workTask.ContactPerson = contact;
  }

  public contactMechanismAdded(partyContactMechanism: PartyContactMechanism): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.workTask.Customer.addPartyContactMechanism(partyContactMechanism);
    this.workTask.FullfillContactMechanism = partyContactMechanism.ContactMechanism;
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
