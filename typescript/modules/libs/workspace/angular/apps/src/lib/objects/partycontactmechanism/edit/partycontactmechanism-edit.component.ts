import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { Enumeration, Party, ContactMechanism, PartyContactMechanism } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { InternalOrganisationId } from '@allors/angular/base';
import { IObject } from '@allors/domain/system';
import { Equals, Sort } from '@allors/data/system';
import { TestScope } from '@allors/angular/core';
@Component({
  templateUrl: './partycontactmechanism-edit.component.html',
  providers: [SessionService],
})
export class PartyContactmechanismEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  partyContactMechanism: PartyContactMechanism;
  contactMechanismPurposes: Enumeration[];
  contactMechanisms: ContactMechanism[] = [];
  organisationContactMechanisms: ContactMechanism[];
  ownContactMechanisms: ContactMechanism[] = [];
  title: string;

  private subscription: Subscription;
  party: Party;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PartyContactmechanismEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.ContactMechanismPurpose({
              predicate: new Equals({ propertyType: m.ContactMechanismPurpose.IsActive, value: true }),
              sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PartyContactMechanism({
                object: this.data.id,
                include: {
                  ContactMechanism: {
                    PostalAddress_Country: x,
                  },
                },
              }),
              pull.PartyContactMechanism({
                object: this.data.id,
                name: 'test',
                select: {
                  PartyWherePartyContactMechanism: {
                    PartyContactMechanisms: {
                      include: {
                        ContactMechanism: {
                          PostalAddress_Country: x,
                        },
                      }
                    },
                  },
                },
              }),
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Party({
                object: this.data.associationId,
              }),
              pull.Person({
                object: this.data.associationId,
                select: {
                  CurrentOrganisationContactMechanisms: {
                    include: {
                      PostalAddress_Country: x,
                    },
                  },
                },
              }),
              pull.PartyContactMechanism({
                object: this.data.id,
                name: 'test',
                select: {
                  PartyWherePartyContactMechanism: {
                    PartyContactMechanisms: {
                      include: {
                        ContactMechanism: {
                          PostalAddress_Country: x,
                        },
                      }
                    },
                  },
                },
              }),
            );
          }

          return this.allors.context.load(new PullRequest({ pulls })).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.contactMechanisms = [];
        this.ownContactMechanisms = [];

        this.contactMechanismPurposes = loaded.collections.ContactMechanismPurposes as Enumeration[];
        this.organisationContactMechanisms = loaded.collections.CurrentOrganisationContactMechanisms as ContactMechanism[];

        const partyContactMechanisms = loaded.collections.test as PartyContactMechanism[];
        partyContactMechanisms.forEach((v) => this.ownContactMechanisms.push(v.ContactMechanism));

        if (this.organisationContactMechanisms !== undefined) {
          this.contactMechanisms = this.contactMechanisms.concat(this.organisationContactMechanisms);
        }

        if (this.ownContactMechanisms !== undefined) {
          this.contactMechanisms = this.contactMechanisms.concat(this.ownContactMechanisms);
        }

        if (isCreate) {
          this.title = 'Add Party ContactMechanism';

          this.partyContactMechanism = this.allors.context.create('PartyContactMechanism') as PartyContactMechanism;
          this.partyContactMechanism.FromDate = new Date().toISOString();
          this.partyContactMechanism.UseAsDefault = true;

          this.party = loaded.objects.Party as Party;
          this.party.AddPartyContactMechanism(this.partyContactMechanism);
        } else {
          this.partyContactMechanism = loaded.objects.PartyContactMechanism as PartyContactMechanism;

          if (this.partyContactMechanism.CanWriteComment) {
            this.title = 'Edit Party ContactMechanism';
          } else {
            this.title = 'View Party ContactMechanism';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.save().subscribe(() => {
      const data: IObject = {
        id: this.partyContactMechanism.id,
        objectType: this.partyContactMechanism.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
