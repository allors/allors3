import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchasereturn-create-form.component.html',
  providers: [ContextService],
})
export class PurchaseReturnCreateFormComponent implements OnInit, OnDestroy {
  readonly m: M;

  title = 'Add Purchase Return';

  purchaseReturn: PurchaseReturn;
  currencies: Currency[];
  shipToAddresses: PostalAddress[] = [];
  shipToContacts: Person[] = [];
  shipFromAddresses: PostalAddress[] = [];
  shipFromContacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  addShipToAddress = false;
  addShipToContactPerson = false;

  private subscription: Subscription;
  facilities: Facility[];

  suppliersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseReturnCreateFormComponent>,
    private refreshService: RefreshService,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.ownWarehouses,
            pull.Organisation({
              predicate: {
                kind: 'Equals',
                propertyType: m.Organisation.IsInternalOrganisation,
                value: true,
              },
              sorting: [{ roleType: m.Organisation.DisplayName }],
            }),
          ];

          this.suppliersFilter = Filters.suppliersFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.facilities = this.fetcher.getOwnWarehouses(loaded);

        this.purchaseReturn = this.allors.context.create<PurchaseReturn>(
          m.PurchaseReturn
        );
        this.purchaseReturn.ShipFromParty = this.internalOrganisation;

        if (this.facilities.length > 0) {
          this.purchaseReturn.ShipFromFacility = this.facilities[0];
        }

        if (this.purchaseReturn.ShipToParty) {
          this.updateShipToParty(this.purchaseReturn.ShipToParty);
        }

        if (this.purchaseReturn.ShipFromParty) {
          this.updateShipFromParty(this.purchaseReturn.ShipFromParty);
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.purchaseReturn);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  public shipToContactPersonAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.purchaseReturn
      .ShipToParty as Organisation;
    organisationContactRelationship.Contact = person;

    this.shipToContacts.push(person);
    this.purchaseReturn.ShipToContactPerson = person;
  }

  public shipToAddressAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.purchaseReturn.ShipToParty.addPartyContactMechanism(
      partyContactMechanism
    );

    const postalAddress =
      partyContactMechanism.ContactMechanism as PostalAddress;
    this.shipToAddresses.push(postalAddress);
    this.purchaseReturn.ShipToAddress = postalAddress;
  }

  public supplierSelected(supplier: IObject) {
    this.updateShipToParty(supplier as Party);
  }

  private updateShipToParty(supplier: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: supplier,
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
        object: supplier,
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
      this.shipToAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map(
          (v: PartyContactMechanism) => v.ContactMechanism
        ) as PostalAddress[];
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }

  private updateShipFromParty(organisation: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: organisation,
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
        object: organisation,
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
      this.shipFromAddresses = partyContactMechanisms
        ?.filter(
          (v: PartyContactMechanism) =>
            v.ContactMechanism.strategy.cls === m.PostalAddress
        )
        ?.map(
          (v: PartyContactMechanism) => v.ContactMechanism
        ) as PostalAddress[];
      this.shipToContacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
