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
  SerialisedItem,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'serialiseditem-edit-form',
  templateUrl: './serialiseditem-edit-form.component.html',
  providers: [OldPanelService, ContextService],
})
export class SerialisedItemEditFormComponent
  extends AllorsFormComponent<SerialisedItem>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  serialisedItem: SerialisedItem;

  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  serialisedItemStates: Enumeration[];
  ownerships: Enumeration[];
  part: Part;
  currentSuppliers: Organisation[];
  currentFacility: Facility;

  private subscription: Subscription;
  serialisedItemAvailabilities: Enumeration[];
  internalOrganisationsFilter: SearchFactory;
  partiesFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private snackBar: MatSnackBar,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    panel.onPull = (pulls) => {
      this.serialisedItem = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          pull.SerialisedItem({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.serialisedItem = loaded.object<SerialisedItem>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;

    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.serialisedItem = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            pull.SerialisedItem({
              objectId: id,
              include: {
                SerialisedItemState: x,
                SerialisedItemCharacteristics: {
                  SerialisedItemCharacteristicType: {
                    UnitOfMeasure: x,
                  },
                },
                LocalisedNames: {
                  Locale: x,
                },
                LocalisedDescriptions: {
                  Locale: x,
                },
                LocalisedComments: {
                  Locale: x,
                },
                LocalisedKeywords: {
                  Locale: x,
                },
                Ownership: x,
                Buyer: x,
                Seller: x,
                OwnedBy: x,
                RentedBy: x,
                PrimaryPhoto: x,
                SecondaryPhotos: x,
                AdditionalPhotos: x,
                PrivatePhotos: x,
                PublicElectronicDocuments: x,
                PrivateElectronicDocuments: x,
                PublicLocalisedElectronicDocuments: x,
                PrivateLocalisedElectronicDocuments: x,
                PurchaseInvoice: x,
                PurchaseOrder: x,
                SuppliedBy: x,
                AssignedSuppliedBy: x,
              },
            }),
            this.fetcher.locales,
            pull.SerialisedItem({
              objectId: id,
              select: {
                PartWhereSerialisedItem: {
                  include: { SerialisedItems: x },
                },
              },
            }),
            pull.SerialisedItem({
              objectId: id,
              select: {
                SerialisedInventoryItemsWhereSerialisedItem: {
                  include: {
                    Facility: x,
                  },
                },
              },
            }),
            pull.InternalOrganisation({
              objectId: this.internalOrganisationId.value,
              select: {
                ObsoleteCurrentSuppliers: x,
              },
            }),
            pull.SerialisedItemState({
              predicate: {
                kind: 'Equals',
                propertyType: m.SerialisedItemState.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.SerialisedItemState.Name }],
            }),
            pull.SerialisedItemAvailability({
              predicate: {
                kind: 'Equals',
                propertyType: m.SerialisedItemAvailability.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.SerialisedItemAvailability.Name }],
            }),
            pull.Ownership({
              predicate: {
                kind: 'Equals',
                propertyType: m.Ownership.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Ownership.Name }],
            }),
          ];

          this.internalOrganisationsFilter =
            Filters.internalOrganisationsFilter(m);
          this.partiesFilter = Filters.partiesFilter(m);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.currentSuppliers = loaded.collection<Organisation>(
          m.InternalOrganisation.ObsoleteCurrentSuppliers
        );

        this.serialisedItem = loaded.object<SerialisedItem>(m.SerialisedItem);
        this.locales = this.fetcher.getAdditionalLocales(loaded);
        this.serialisedItemStates = loaded.collection<SerialisedItemState>(
          m.SerialisedItemState
        );
        this.serialisedItemAvailabilities =
          loaded.collection<SerialisedItemAvailability>(
            m.SerialisedItemAvailability
          );
        this.ownerships = loaded.collection<Ownership>(m.Ownership);
        this.part = loaded.object<Part>(
          m.SerialisedItem.PartWhereSerialisedItem
        );

        const serialisedInventoryItems =
          loaded.collection<SerialisedInventoryItem>(
            m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem
          );
        const inventoryItem = serialisedInventoryItems?.find(
          (v) => v.Quantity === 1
        );
        if (inventoryItem) {
          this.currentFacility = inventoryItem.Facility;
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public partSelected(part: Part): void {
    if (part) {
      this.part = part;
    }
  }

  public save(): void {
    // this.onSave();

    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.errorService.errorHandler);
  }

  public update(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  private onSave() {
    this.part.addSerialisedItem(this.serialisedItem);
  }
}
