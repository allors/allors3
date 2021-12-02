import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PartyContactMechanism, Party, PostalAddress, Country, Enumeration, ContactMechanismPurpose } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './postaladdress-create.component.html',
  providers: [ContextService],
})
export class PostalAddressCreateComponent implements OnInit, OnDestroy {
  readonly m: M;

  public title = 'Add Postal Address';

  contactMechanism: PostalAddress;
  countries: Country[];
  party: Party;
  contactMechanismPurposes: Enumeration[];
  partyContactMechanism: PartyContactMechanism;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PostalAddressCreateComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const pulls = [
            pull.Party({
              objectId: this.data.associationId,
              include: { PartyContactMechanisms: x },
            }),
            pull.Country({
              sorting: [{ roleType: m.Country.Name }],
            }),
            pull.ContactMechanismPurpose({
              predicate: { kind: 'Equals', propertyType: m.ContactMechanismPurpose.IsActive, value: true },
              sorting: [{ roleType: this.m.ContactMechanismPurpose.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.countries = loaded.collection<Country>(m.Country);
        this.contactMechanismPurposes = loaded.collection<ContactMechanismPurpose>(m.ContactMechanismPurpose);
        this.party = loaded.object<Party>(m.Party);

        this.contactMechanism = this.allors.context.create<PostalAddress>(m.PostalAddress);

        this.partyContactMechanism = this.allors.context.create<PartyContactMechanism>(m.PartyContactMechanism);
        this.partyContactMechanism.UseAsDefault = true;
        this.partyContactMechanism.ContactMechanism = this.contactMechanism;

        this.party.addPartyContactMechanism(this.partyContactMechanism);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.contactMechanism);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
