import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  Part,
  SupplierOffering,
  Facility,
  InternalOrganisation,
  NonSerialisedInventoryItem,
  InventoryItem,
  SerialisedInventoryItem,
  SerialisedItem,
  PurchaseOrder,
  PurchaseOrderItem,
  UnifiedGood,
  VatRegime,
  IrpfRegime,
  InvoiceItemType,
  Product,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { And, IObject } from '@allors/system/workspace/domain';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchaseorderitem-edit.component.html',
  providers: [ContextService],
})
export class PurchaseOrderItemEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  title: string;
  order: PurchaseOrder;
  orderItem: PurchaseOrderItem;
  inventoryItems: InventoryItem[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  invoiceItemTypes: InvoiceItemType[];
  partItemType: InvoiceItemType;
  productItemType: InvoiceItemType;
  serviceItemType: InvoiceItemType;
  timeItemType: InvoiceItemType;
  part: Part;
  serialisedItems: SerialisedItem[];
  serialisedItem: SerialisedItem;
  serialised: boolean;
  nonUnifiedPart: boolean;
  unifiedGood: boolean;
  addFacility = false;
  supplierOffering: SupplierOffering;
  facilities: Facility[];
  selectedFacility: Facility;
  showIrpf: boolean;
  vatRegimeInitialRole: VatRegime;
  irpfRegimeInitialRole: IrpfRegime;

  private subscription: Subscription;
  partsFilter: SearchFactory;

  unifiedGoodsFilter: SearchFactory;
  internalOrganisation: InternalOrganisation;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseOrderItemEditComponent>,
    private fetcher: FetcherService,
    public refreshService: RefreshService,
    private errorService: ErrorService
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull, treeBuilder } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const { id } = this.data;

          const pulls = [
            this.fetcher.internalOrganisation,
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
            pull.Facility({
              sorting: [{ roleType: m.Facility.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PurchaseOrderItem({
                objectId: id,
                include: {
                  InvoiceItemType: x,
                  PurchaseOrderItemState: x,
                  PurchaseOrderItemShipmentState: x,
                  PurchaseOrderItemPaymentState: x,
                  Part: x,
                  SerialisedItem: x,
                  StoredInFacility: x,
                  DerivedVatRegime: {
                    VatRates: x,
                  },
                  DerivedIrpfRegime: {
                    IrpfRates: x,
                  },
                },
              }),
              pull.PurchaseOrderItem({
                objectId: id,
                select: {
                  PurchaseOrderWherePurchaseOrderItem: {
                    include: {
                      PurchaseOrderItems: x,
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
              pull.PurchaseOrder({
                objectId: this.data.associationId,
                include: {
                  PurchaseOrderItems: x,
                  DerivedVatRegime: x,
                  DerivedIrpfRegime: x,
                  TakenViaSupplier: x,
                },
              })
            );
          }

          this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(m, treeBuilder);

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
        this.orderItem = loaded.object<PurchaseOrderItem>(m.PurchaseOrderItem);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.facilities = loaded.collection<Facility>(m.Facility);

        this.invoiceItemTypes = loaded.collection<InvoiceItemType>(
          m.InvoiceItemType
        );
        this.partItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b'
        );
        this.productItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad'
        );
        this.serviceItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === 'a4d2e6d0-c6c1-46ec-a1cf-3a64822e7a9e'
        );
        this.timeItemType = this.invoiceItemTypes?.find(
          (v: InvoiceItemType) =>
            v.UniqueId === 'da178f93-234a-41ed-815c-819af8ca4e6f'
        );

        this.partsFilter = new SearchFactory({
          objectType: this.m.NonUnifiedPart,
          roleTypes: [
            this.m.NonUnifiedPart.Name,
            this.m.NonUnifiedPart.SearchString,
          ],
          post: (predicate: And) => {
            predicate.operands.push({
              kind: 'ContainedIn',
              propertyType: this.m.NonUnifiedPart.SupplierOfferingsWherePart,
              extent: {
                kind: 'Filter',
                objectType: this.m.SupplierOffering,
                predicate: {
                  kind: 'And',
                  operands: [
                    {
                      kind: 'Equals',
                      propertyType: m.SupplierOffering.Supplier,
                      object: this.order.TakenViaSupplier,
                    },
                    {
                      kind: 'LessThan',
                      roleType: m.SupplierOffering.FromDate,
                      value: this.order.OrderDate,
                    },
                    {
                      kind: 'Or',
                      operands: [
                        {
                          kind: 'Not',
                          operand: {
                            kind: 'Exists',
                            propertyType: m.SupplierOffering.ThroughDate,
                          },
                        },
                        {
                          kind: 'GreaterThan',
                          roleType: m.SupplierOffering.ThroughDate,
                          value: this.order.OrderDate,
                        },
                      ],
                    },
                  ],
                },
              },
            });
          },
        });

        if (isCreate) {
          this.title = 'Add Purchase Order Item';
          this.order = loaded.object<PurchaseOrder>(m.PurchaseOrder);
          this.orderItem = this.allors.context.create<PurchaseOrderItem>(
            m.PurchaseOrderItem
          );
          this.selectedFacility = this.order.StoredInFacility;
          this.order.addPurchaseOrderItem(this.orderItem);
          this.vatRegimeInitialRole = this.order.DerivedVatRegime;
          this.irpfRegimeInitialRole = this.order.DerivedIrpfRegime;
        } else {
          this.order = this.orderItem.PurchaseOrderWherePurchaseOrderItem;
          this.selectedFacility = this.orderItem.StoredInFacility;

          if (this.orderItem.Part) {
            this.unifiedGood =
              this.orderItem.Part.strategy.cls === m.UnifiedGood;
            this.nonUnifiedPart =
              this.orderItem.Part.strategy.cls === m.NonUnifiedPart;
            this.updateFromPart(this.orderItem.Part);
          }

          if (this.orderItem.canWriteQuantityOrdered) {
            this.title = 'Edit Purchase Order Item';
          } else {
            this.title = 'View Purchase Order Item';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public goodSelected(unifiedGood: IObject): void {
    if (unifiedGood) {
      this.part = unifiedGood as UnifiedGood;
      this.refreshSerialisedItems(unifiedGood as UnifiedGood);
    }
  }

  public serialisedItemSelected(serialisedItem: IObject): void {
    if (serialisedItem) {
      this.serialisedItem = this.part.SerialisedItems?.find(
        (v) => v === serialisedItem
      );
      this.orderItem.QuantityOrdered = '1';
    }
  }

  public partSelected(part: IObject): void {
    if (part) {
      this.unifiedGood =
        this.orderItem.Part.strategy.cls === this.m.UnifiedGood;
      this.nonUnifiedPart =
        this.orderItem.Part.strategy.cls === this.m.NonUnifiedPart;

      this.updateFromPart(part as Part);
    }
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }

  public save(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.orderItem);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }

  private refreshSerialisedItems(product: Product): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedGood({
        objectId: product.id,
        select: {
          Part: {
            include: {
              SerialisedItems: x,
              InventoryItemKind: x,
            },
          },
        },
      }),
      pull.UnifiedGood({
        objectId: product.id,
        include: {
          InventoryItemKind: x,
          SerialisedItems: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe(() => {
      this.serialisedItems = this.part.SerialisedItems;
      this.serialised =
        this.part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
    });
  }

  private updateFromPart(part: Part) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Part({
        object: part,
        select: {
          SerialisedItems: {
            include: {
              OwnedBy: x,
            },
          },
        },
      }),
      pull.Part({
        object: part,
        select: {
          SupplierOfferingsWherePart: {
            include: {
              Supplier: x,
            },
          },
        },
      }),
      pull.Part({
        object: part,
        include: {
          InventoryItemKind: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.part = (loaded.object<UnifiedGood>(m.UnifiedGood) ||
        loaded.object<Part>(m.Part)) as Part;
      this.serialised =
        part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      const supplierOfferings = loaded.collection<SupplierOffering>(
        m.Part.SupplierOfferingsWherePart
      );
      this.supplierOffering = supplierOfferings?.find(
        (v) =>
          isBefore(new Date(v.FromDate), new Date()) &&
          (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())) &&
          v.Supplier === this.order.TakenViaSupplier
      );

      this.serialisedItems = loaded.collection<SerialisedItem>(
        m.Part.SerialisedItems
      );

      if (this.orderItem.SerialisedItem) {
        this.serialisedItems.push(this.orderItem.SerialisedItem);
      }
    });
  }

  private onSave() {
    if (
      this.orderItem.InvoiceItemType !== this.partItemType &&
      this.orderItem.InvoiceItemType !== this.partItemType
    ) {
      this.orderItem.QuantityOrdered = '1';
    }
  }
}
