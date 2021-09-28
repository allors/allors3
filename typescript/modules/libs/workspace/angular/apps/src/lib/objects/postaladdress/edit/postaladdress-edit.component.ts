import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Carrier,  Person, Organisation, PartyContactMechanism, OrganisationContactRelationship, Party, CustomerShipment, Currency, PostalAddress, Facility, ShipmentMethod, PositionTypeRate, TimeFrequency, RateType, PositionType, PriceComponent, Country, ContactMechanismPurpose, ContactMechanism } from '@allors/workspace/domain/default';
import { Action, DeleteService, EditService, NavigationService, ObjectData, PanelService, RefreshService, SaveService, SearchFactory, Table, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './postaladdress-edit.component.html',
  providers: [SessionService]
})
export class PostalAddressEditComponent extends TestScope implements OnInit, OnDestroy {

  readonly m: M;

  contactMechanism: PostalAddress;
  countries: Country[];
  title: string;

  private subscription: Subscription;
  party: Party;
  partyContactMechanism: PartyContactMechanism;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: IObject,
    public dialogRef: MatDialogRef<PostalAddressEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
  ) {

    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {

          const pulls = [
            pull.ContactMechanism({
              objectId: this.data.id,
              include: {
                PostalAddress_Country: x
              },
            }),
            pull.Country({
              sorting: [{ roleType: m.Country.Name }]
            })
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {

        this.allors.session.reset();

        this.countries = loaded.collection<Country>(m.Country);
        this.contactMechanism = loaded.object<ContactMechanism>(m.ContactMechanism);

        if (this.contactMechanism.canWriteAddress1) {
          this.title = 'Edit Postal Address';
        } else {
          this.title = 'View Postal Address';
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.allors.client.pushReactive(this.allors.session)
      .subscribe(() => {
        const data: IObject = {
          id: this.contactMechanism.id,
          objectType: this.contactMechanism.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
