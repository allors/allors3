import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { ContextService, TestScope, MetaService, RefreshService, Context, Saved, NavigationService } from '@allors/angular/core';
import { ElectronicAddress, Enumeration, Employment, Person, Party, Organisation, CommunicationEventPurpose, FaceToFaceCommunication, CommunicationEventState, OrganisationContactRelationship, InventoryItem, InternalOrganisation, InventoryItemTransaction, InventoryTransactionReason, Part, Facility, Lot, SerialisedInventoryItem, SerialisedItem, NonSerialisedInventoryItemState, SerialisedInventoryItemState, NonSerialisedInventoryItem, ContactMechanism, LetterCorrespondence, PartyContactMechanism, PostalAddress, OrderAdjustment, OrganisationContactKind, PartyRate, TimeFrequency, RateType, PhoneCommunication, TelecommunicationsNumber, PositionType, PositionTypeRate, ProductIdentification, ProductIdentificationType, ProductType, SerialisedItemCharacteristicType } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta, ids } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/core';
import { InternalOrganisationId, FetcherService, FiltersService } from '@allors/angular/base';
import { IObject, ISessionObject } from '@allors/domain/system';
import { Equals, Sort } from '@allors/data/system';

@Component({
  templateUrl: './producttype-edit.component.html',
  providers: [ContextService]
})
export class ProductTypeEditComponent extends TestScope implements OnInit, OnDestroy {

  public title: string;
  public subTitle: string;

  public m: Meta;

  public productType: ProductType;

  public characteristics: SerialisedItemCharacteristicType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ProductTypeEditComponent>,
    public metaService: MetaService,
    public refreshService: RefreshService,
    private saveService: SaveService,
  ) {

    super();

    this.m = this.metaService.m;
  }

  public ngOnInit(): void {

    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.ProductType({
              object: this.data.id,
              include: {
                SerialisedItemCharacteristicTypes: x,
              }
            }),
            pull.SerialisedItemCharacteristicType({
              sort: new Sort(m.SerialisedItemCharacteristicType.Name),
            })
          ];

          return this.allors.context
            .load(new PullRequest({ pulls }))
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {

        this.allors.context.reset();

        this.characteristics = loaded.collections.SerialisedItemCharacteristicTypes as SerialisedItemCharacteristicType[];

        if (isCreate) {
          this.title = 'Add Product Type';
          this.productType = this.allors.context.create('ProductType') as ProductType;
        } else {
          this.productType = loaded.objects.ProductType as ProductType;

          if (this.productType.CanWriteName) {
            this.title = 'Edit Product Type';
          } else {
            this.title = 'View Product Type';
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

    this.allors.context
      .save()
      .subscribe((saved: Saved) => {
        const data: IObject = {
          id: this.productType.id,
          objectType: this.productType.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}