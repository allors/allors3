import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  Part,
  SupplierOffering,
  Facility,
  NonUnifiedPart,
  InternalOrganisation,
  NonSerialisedInventoryItem,
  InventoryItem,
  SerialisedInventoryItem,
  SerialisedItem,
  UnifiedGood,
  SerialisedItemAvailability,
  SalesOrderItem,
  SalesInvoice,
  VatRegime,
  IrpfRegime,
  InvoiceItemType,
  SalesInvoiceItem,
  Product,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './salesinvoiceitem-edit.component.html',
  providers: [ContextService],
})
export class SalesInvoiceItemEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title: string;
  invoice: SalesInvoice;
  invoiceItem: SalesInvoiceItem;
  orderItem: SalesOrderItem;
  inventoryItems: InventoryItem[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  invoiceItemTypes: InvoiceItemType[];
  productItemType: InvoiceItemType;
  facilities: Facility[];
  goodsFacilityFilter: SearchFactory;
  part: Part;
  serialisedItem: SerialisedItem;
  serialisedItems: SerialisedItem[] = [];
  serialisedItemAvailabilities: SerialisedItemAvailability[];

  private previousProduct;
  private subscription: Subscription;
  parts: NonUnifiedPart[];
  partItemType: InvoiceItemType;
  supplierOffering: SupplierOffering;
  inRent: SerialisedItemAvailability;

  goodsFilter: SearchFactory;
  internalOrganisation: InternalOrganisation;
  showIrpf: boolean;
  vatRegimeInitialRole: VatRegime;
  irpfRegimeInitialRole: IrpfRegime;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SalesInvoiceItemEditComponent>,
    public refreshService: RefreshService,

    private fetcher: FetcherService,
    private saveService: SaveService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    this.goodsFacilityFilter = new SearchFactory({
      objectType: this.m.Good,
      roleTypes: [this.m.Good.Name],
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const { id } = this.data;

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.warehouses,
            pull.SerialisedItemAvailability({}),
            pull.InvoiceItemType({
              predicate: {
                kind: 'Equals',
                propertyType: m.InvoiceItemType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.InvoiceItemType.Name }],
            }),
            pull.IrpfRegime({
              sorting: [{ roleType: m.IrpfRegime.Name }],
            }),
            pull.SerialisedItemAvailability({}),
          ];

          if (!isCreate) {
            pulls.push(
              pull.SalesInvoiceItem({
                objectId: id,
                include: {
                  SalesInvoiceItemState: x,
                  SerialisedItem: x,
                  NextSerialisedItemAvailability: x,
                  Facility: {
                    Owner: x,
                  },
                  DerivedVatRegime: {
                    VatRates: x,
                  },
                  DerivedIrpfRegime: {
                    IrpfRates: x,
                  },
                },
              }),
              pull.SalesInvoiceItem({
                objectId: id,
                select: {
                  SalesInvoiceWhereSalesInvoiceItem: {
                    include: {
                      SalesInvoiceItems: x,
                      DerivedVatRegime: {
                        VatRates: x,
                      },
                      DerivedIrpfRegime: {
                        IrpfRates: x,
                      },
                    },
                  },
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.SalesInvoice({
                objectId: this.data.associationId,
                include: {
                  SalesInvoiceItems: x,
                  DerivedVatRegime: {
                    VatRates: x,
                  },
                  DerivedIrpfRegime: {
                    IrpfRates: x,
                  },
                },
              })
            );
          }

          this.goodsFilter = Filters.goodsFilter(m);

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.invoiceItem = loaded.object<SalesInvoiceItem>(m.SalesInvoiceItem);
        this.orderItem = loaded.object<SalesOrderItem>(m.SalesOrderItem);
        this.parts = loaded.collection<NonUnifiedPart>(m.NonUnifiedPart);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.serialisedItemAvailabilities =
          loaded.collection<SerialisedItemAvailability>(
            m.SerialisedItemAvailability
          );
        this.facilities = this.fetcher.getWarehouses(loaded);
        this.invoiceItemTypes = loaded.collection<InvoiceItemType>(
          m.InvoiceItemType
        );
        this.productItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad'
        );
        this.partItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b'
        );

        const serialisedItemAvailabilities =
          loaded.collection<SerialisedItemAvailability>(
            m.SerialisedItemAvailability
          );
        this.inRent = serialisedItemAvailabilities?.find(
          (v: SerialisedItemAvailability) =>
            v.UniqueId === 'ec87f723-2284-4f5c-ba57-fcf328a0b738'
        );

        if (isCreate) {
          this.title = 'Add sales invoice Item';
          this.invoice = loaded.object<SalesInvoice>(m.SalesInvoice);
          this.invoiceItem = this.allors.context.create<SalesInvoiceItem>(
            m.SalesInvoiceItem
          );
          this.invoice.addSalesInvoiceItem(this.invoiceItem);
          this.vatRegimeInitialRole = this.invoice.DerivedVatRegime;
          this.irpfRegimeInitialRole = this.invoice.DerivedIrpfRegime;
        } else {
          this.title = 'Edit invoice Item';
          this.invoice = this.invoiceItem.SalesInvoiceWhereSalesInvoiceItem;

          this.previousProduct = this.invoiceItem.Product;
          this.serialisedItem = this.invoiceItem.SerialisedItem;

          if (this.invoiceItem.InvoiceItemType === this.productItemType) {
            this.goodSelected(this.invoiceItem.Product);
          }

          if (this.invoiceItem.canWriteQuantity) {
            this.title = 'Edit invoice Item';
          } else {
            this.title = 'View invoice Item';
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
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.invoiceItem);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  public goodSelected(object: any) {
    if (object) {
      this.refreshSerialisedItems(object as Product);
    }
  }

  public serialisedItemSelected(serialisedItem: IObject): void {
    const unifiedGood = this.invoiceItem.Product as UnifiedGood;
    this.serialisedItem = unifiedGood.SerialisedItems?.find(
      (v) => v === serialisedItem
    );
    this.invoiceItem.AssignedUnitPrice = this.serialisedItem.ExpectedSalesPrice;
    this.invoiceItem.Quantity = '1';
  }

  private refreshSerialisedItems(good: Product): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const unifiedGoodPullName = `${this.m.UnifiedGood.tag}_items`;
    const nonUnifiedGoodPullName = `${this.m.NonUnifiedGood.tag}_items`;

    const pulls = [
      pull.NonUnifiedGood({
        name: nonUnifiedGoodPullName,
        objectId: good.id,
        select: {
          Part: {
            SerialisedItems: {
              include: {
                SerialisedItemAvailability: x,
                PartWhereSerialisedItem: x,
              },
            },
          },
        },
      }),
      pull.UnifiedGood({
        name: unifiedGoodPullName,
        objectId: good.id,
        select: {
          SerialisedItems: {
            include: {
              SerialisedItemAvailability: x,
              PartWhereSerialisedItem: x,
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const serialisedItems1 =
        loaded.collection<SerialisedItem>(unifiedGoodPullName);
      const serialisedItems2 = loaded.collection<SerialisedItem>(
        nonUnifiedGoodPullName
      );
      const items = serialisedItems1 || serialisedItems2;

      this.serialisedItems = items?.filter(
        (v) =>
          v.AvailableForSale === true ||
          v.SerialisedItemAvailability === this.inRent
      );

      if (this.invoiceItem.Product !== this.previousProduct) {
        this.invoiceItem.SerialisedItem = null;
        this.serialisedItem = null;
        this.previousProduct = this.invoiceItem.Product;
      }
    });
  }
}
