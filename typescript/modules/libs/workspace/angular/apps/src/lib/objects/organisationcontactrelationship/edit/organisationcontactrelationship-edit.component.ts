import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Person, Organisation, OrganisationContactRelationship, Party, OrganisationContactKind } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './organisationcontactrelationship-edit.component.html',
  providers: [ContextService],
})
export class OrganisationContactRelationshipEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyRelationship: OrganisationContactRelationship;
  title: string;
  addContact = false;

  private subscription: Subscription;
  party: Party;
  person: Person;
  organisation: Organisation;
  organisations: Organisation[];
  contactKinds: OrganisationContactKind[];
  generalContact: OrganisationContactKind;

  peopleFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<OrganisationContactRelationshipEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;  const { pullBuilder: pull } = m; const x = {};
    // this.filters = Filters;

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [
            pull.Organisation({}),
            pull.OrganisationContactKind({
              sorting: [{ roleType: this.m.OrganisationContactKind.Description }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.OrganisationContactRelationship({
                objectId: this.data.id,
                include: {
                  ContactKinds: x,
                  Organisation: x,
                  Contact: x,
                  Parties: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                objectId: this.data.associationId,
              })
            );
          }

          this.peopleFilter = Filters.peopleFilter(m);

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.organisations = loaded.collection<Organisation>(m.Organisation);

        this.contactKinds = loaded.collection<OrganisationContactKind>(m.OrganisationContactKind);
        this.generalContact = this.contactKinds?.find((v) => v.UniqueId === 'eebe4d65-c452-49c9-a583-c0ffec385e98');

        if (isCreate) {
          this.title = 'Add Organisation Contact';

          this.partyRelationship = this.allors.context.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
          this.partyRelationship.FromDate = new Date();
          this.partyRelationship.addContactKind(this.generalContact);

          this.party = loaded.object<Party>(m.Party);

          if (this.party.strategy.cls === m.Person) {
            this.person = this.party as Person;
            this.partyRelationship.Contact = this.person;
          }

          if (this.party.strategy.cls === m.Organisation) {
            this.organisation = this.party as Organisation;
            this.partyRelationship.Organisation = this.organisation;
          }
        } else {
          this.partyRelationship = loaded.object<OrganisationContactRelationship>(m.OrganisationContactRelationship);
          this.person = this.partyRelationship.Contact;
          this.organisation = this.partyRelationship.Organisation as Organisation;

          if (this.partyRelationship.canWriteFromDate) {
            this.title = 'Edit Organisation Contact';
          } else {
            this.title = 'View Organisation Contact';
          }
        }
      });
  }

  public contactAdded(contact: Person): void {
    this.partyRelationship.Contact = contact;
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.partyRelationship);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
