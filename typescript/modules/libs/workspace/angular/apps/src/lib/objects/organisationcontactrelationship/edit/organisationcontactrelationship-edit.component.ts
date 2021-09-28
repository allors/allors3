import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { Person, Party, Organisation, OrganisationContactRelationship, OrganisationContactKind } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { InternalOrganisationId, Filters } from '@allors/angular/base';
import { IObject } from '@allors/domain/system';
import { Sort } from '@allors/data/system';
import { TestScope, SearchFactory } from '@allors/angular/core';

@Component({
  templateUrl: './organisationcontactrelationship-edit.component.html',
  providers: [SessionService],
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
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<OrganisationContactRelationshipEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pull, x, m } = this.metaService;
    // this.filters = Filters;

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.Organisation(),
            pull.OrganisationContactKind({
              sorting: [{ roleType: this.m.OrganisationContactKind.Description }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.OrganisationContactRelationship({
                objectId: this.data.id,
                include: {
                  Organisation: x,
                  Contact: x,
                  Parties: x,
                },
              }),
            );
          }


          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                object: this.data.associationId,
              }),
            );
          }

          this.peopleFilter = Filters.peopleFilter(m);

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.organisations = loaded.collection<Organisation>(m.Organisation);

        this.contactKinds = loaded.collection<OrganisationContactKind>(m.OrganisationContactKind);
        this.generalContact = this.contactKinds.find((v) => v.UniqueId === 'eebe4d65-c452-49c9-a583-c0ffec385e98');

        if (isCreate) {
          this.title = 'Add Organisation Contact';

          this.partyRelationship = this.allors.session.create<OrganisationContactRelationship>(m.OrganisationContactRelationship);
          this.partyRelationship.FromDate = new Date();;
          this.partyRelationship.AddContactKind(this.generalContact);

          this.party = loaded.object<Party>(m.Party);

          if (this.party.objectType.name === m.Person.name) {
            this.person = this.party as Person;
            this.partyRelationship.Contact = this.person;
          }

          if (this.party.objectType.name === m.Organisation.name) {
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
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.partyRelationship.id,
        objectType: this.partyRelationship.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
