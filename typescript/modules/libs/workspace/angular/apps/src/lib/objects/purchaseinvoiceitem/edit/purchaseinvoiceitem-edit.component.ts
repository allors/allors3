import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { isBefore, isAfter } from 'date-fns';

import { M } from '@allors/workspace/meta/default';
import {
  Organisation,
  Part,
  SupplierOffering,
  InternalOrganisation,
  NonSerialisedInventoryItem,
  InventoryItem,
  SerialisedInventoryItem,
  SerialisedItem,
  PurchaseOrderItem,
  UnifiedGood,
  VatRegime,
  IrpfRegime,
  InvoiceItemType,
  PurchaseInvoice,
  PurchaseInvoiceItem,
} from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, SearchFactory, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './purchaseinvoiceitem-edit.component.html',
  providers: [SessionService],
})
export class PurchaseInvoiceItemEditComponent extends TestScope implements OnInit, OnDestroy {
  m: M;

  title: string;
  invoice: PurchaseInvoice;
  invoiceItem: PurchaseInvoiceItem;
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
  showIrpf: boolean;
  vatRegimeInitialRole: VatRegime;
  irpfRegimeInitialRole: IrpfRegime;

  private subscription: Subscription;
  partsFilter: SearchFactory;
  supplierOffering: SupplierOffering;

  unifiedGoodsFilter: SearchFactory;
  internalOrganisation: Organisation;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PurchaseInvoiceItemEditComponent>,

    private fetcher: FetcherService,
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.allors.workspace.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;
          const { id } = this.data;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.InvoiceItemType({
              predicate: { kind: 'Equals', propertyType: m.InvoiceItemType.IsActive, value: true },
              sorting: [{ roleType: m.InvoiceItemType.Name }],
            }),
            pull.IrpfRegime({
              sorting: [{ roleType: m.IrpfRegime.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PurchaseInvoiceItem({
                objectId: id,
                include: {
                  PurchaseInvoiceItemState: x,
                  SerialisedItem: x,
                  DerivedVatRegime: {
                    VatRates: x,
                  },
                  DerivedIrpfRegime: {
                    IrpfRates: x,
                  },
                },
              }),
              pull.PurchaseInvoiceItem({
                objectId: id,
                select: {
                  PurchaseInvoiceWherePurchaseInvoiceItem: {
                    include: {
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

          if (this.data.associationId) {
            pulls.push(
              pull.PurchaseInvoice({
                objectId: this.data.associationId,
                include: {
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

          this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(m, this.metaService.tree);

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.invoiceItem = loaded.object<PurchaseInvoiceItem>(m.PurchaseInvoiceItem);
        this.orderItem = loaded.object<PurchaseOrderItem>(m.PurchaseOrderItem);
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.invoiceItemTypes = loaded.collection<InvoiceItemType>(m.InvoiceItemType);
        this.partItemType = this.invoiceItemTypes.find((v: InvoiceItemType) => v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b');
        this.productItemType = this.invoiceItemTypes.find((v: InvoiceItemType) => v.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad');
        this.serviceItemType = this.invoiceItemTypes.find((v: InvoiceItemType) => v.UniqueId === 'a4d2e6d0-c6c1-46ec-a1cf-3a64822e7a9e');
        this.timeItemType = this.invoiceItemTypes.find((v: InvoiceItemType) => v.UniqueId === 'da178f93-234a-41ed-815c-819af8ca4e6f');

        this.partsFilter = new SearchFactory({
          objectType: this.m.Part,
          roleTypes: [this.m.Part.Name, this.m.Part.SearchString],
          post: (predicate: And) => {
            predicate.operands.push(
              new ContainedIn({
                propertyType: this.m.Part.SupplierOfferingsWherePart,
                extent: new Extent({
                  objectType: this.m.SupplierOffering,
                  predicate: { kind: 'Equals', propertyType: m.SupplierOffering.Supplier, object: this.invoice.BilledFrom },
                }),
              })
            );
          },
        });

        if (isCreate) {
          this.title = 'Add purchase invoice Item';
          this.invoice = loaded.object<PurchaseInvoice>(m.PurchaseInvoice);
          this.invoiceItem = this.allors.session.create<PurchaseInvoiceItem>(m.PurchaseInvoiceItem);
          this.invoice.addPurchaseInvoiceItem(this.invoiceItem);
          this.vatRegimeInitialRole = this.invoice.DerivedVatRegime;
          this.irpfRegimeInitialRole = this.invoice.DerivedIrpfRegime;
        } else {
          this.invoice = this.invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem;

          if (this.invoiceItem.Part) {
            this.unifiedGood = this.invoiceItem.Part.objectType.name === m.UnifiedGood.name;
            this.nonUnifiedPart = this.invoiceItem.Part.objectType.name === m.NonUnifiedPart.name;
            this.updateFromPart(this.invoiceItem.Part);
          }

          if (this.invoiceItem.canWriteQuantity) {
            this.title = 'Edit purchase invoice Item';
          } else {
            this.title = 'View purchase invoice Item';
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
    this.serialisedItem = this.part.SerialisedItems.find((v) => v === serialisedItem);
    this.invoiceItem.Quantity = '1';
  }

  public partSelected(part: IObject): void {
    if (part) {
      this.unifiedGood = this.invoiceItem.Part.objectType.name === this.m.UnifiedGood.name;
      this.nonUnifiedPart = this.invoiceItem.Part.objectType.name === this.m.NonUnifiedPart.name;

      this.updateFromPart(part as Part);
    }
  }

  public save(): void {
    this.onSave();

    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.dialogRef.close(this.invoiceItem);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }

  private refreshSerialisedItems(unifiedGood: UnifiedGood): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedGood({
        objectId: unifiedGood.id,
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
        objectId: unifiedGood.id,
        include: {
          InventoryItemKind: x,
          SerialisedItems: x,
        },
      }),
    ];

    this.allors.client.pullReactive(this.allors.session, pulls).subscribe(() => {
      this.serialisedItems = this.part.SerialisedItems;
      this.serialised = this.part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
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

    this.allors.client.pullReactive(this.allors.session, pulls).subscribe((loaded) => {
      this.part = (loaded.objects.UnifiedGood || loaded.objects.Part) as Part;
      this.serialised = part.InventoryItemKind.UniqueId === '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      const supplierOfferings = loaded.collection<SupplierOffering>(m.SupplierOffering);
      this.supplierOffering = supplierOfferings.find((v) => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())) && v.Supplier === this.invoice.BilledFrom);

      this.serialisedItems = loaded.collection<SerialisedItem>(m.SerialisedItem);

      if (this.invoiceItem.SerialisedItem) {
        this.serialisedItems.push(this.invoiceItem.SerialisedItem);
      }
    });
  }

  private onSave() {
    if (this.invoiceItem.InvoiceItemType !== this.partItemType && this.invoiceItem.InvoiceItemType !== this.partItemType) {
      this.invoiceItem.Quantity = '1';
    }
  }
}
