import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  Pull,
  IPullResult,
  And,
  IObject,
} from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  InventoryItem,
  InvoiceItemType,
  IrpfRegime,
  NonSerialisedInventoryItem,
  Part,
  PurchaseInvoice,
  PurchaseInvoiceItem,
  PurchaseOrderItem,
  SerialisedInventoryItem,
  SerialisedItem,
  SupplierOffering,
  UnifiedGood,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M, TreeBuilder } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { isAfter, isBefore } from 'date-fns';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './purchaseinvoiceitem-form.component.html',
  providers: [ContextService],
})
export class PurchaseInvoiceItemFormComponent extends AllorsFormComponent<PurchaseInvoiceItem> {
  m: M;
  readonly treeBuilder: TreeBuilder;

  invoice: PurchaseInvoice;
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
  partsFilter: SearchFactory;
  supplierOffering: SupplierOffering;

  unifiedGoodsFilter: SearchFactory;
  internalOrganisation: InternalOrganisation;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;
    this.treeBuilder = treeBuilder;

    this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(this.m, treeBuilder);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.InvoiceItemType({
        predicate: {
          kind: 'Equals',
          propertyType: m.InvoiceItemType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.InvoiceItemType.Name }],
      }),
      p.IrpfRegime({
        sorting: [{ roleType: m.IrpfRegime.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.PurchaseInvoiceItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            PurchaseInvoiceItemState: {},
            SerialisedItem: {},
            DerivedVatRegime: {
              VatRates: {},
            },
            DerivedIrpfRegime: {
              IrpfRates: {},
            },
          },
        }),
        p.PurchaseInvoiceItem({
          objectId: this.editRequest.objectId,
          select: {
            PurchaseInvoiceWherePurchaseInvoiceItem: {
              include: {
                PurchaseInvoiceItems: {},
                DerivedVatRegime: {
                  VatRates: {},
                },
                DerivedIrpfRegime: {
                  IrpfRates: {},
                },
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.PurchaseInvoice({
          objectId: initializer.id,
          include: {
            PurchaseInvoiceItems: {},
            DerivedVatRegime: {
              VatRates: {},
            },
            DerivedIrpfRegime: {
              IrpfRates: {},
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.orderItem = pullResult.object<PurchaseOrderItem>(
      this.m.PurchaseOrderItem
    );
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.invoiceItemTypes = pullResult.collection<InvoiceItemType>(
      this.m.InvoiceItemType
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

    if (this.createRequest) {
      this.invoice = pullResult.object<PurchaseInvoice>(this.m.PurchaseInvoice);
      this.invoice.addPurchaseInvoiceItem(this.object);
      this.vatRegimeInitialRole = this.invoice.DerivedVatRegime;
      this.irpfRegimeInitialRole = this.invoice.DerivedIrpfRegime;
    } else {
      this.invoice = this.object.PurchaseInvoiceWherePurchaseInvoiceItem;

      if (this.object.Part) {
        this.unifiedGood = this.object.Part.strategy.cls === this.m.UnifiedGood;
        this.nonUnifiedPart =
          this.object.Part.strategy.cls === this.m.NonUnifiedPart;
        this.updateFromPart(this.object.Part);
      }
    }

    this.partsFilter = new SearchFactory({
      objectType: this.m.Part,
      roleTypes: [this.m.Part.Name, this.m.Part.SearchString],
      include: this.treeBuilder.InventoryItemKind({}),
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: this.m.Part.SupplierOfferingsWherePart,
          extent: {
            kind: 'Filter',
            objectType: this.m.SupplierOffering,
            predicate: {
              kind: 'Equals',
              propertyType: this.m.SupplierOffering.Supplier,
              object: this.invoice.BilledFrom,
            },
          },
        });
      },
    });
  }

  public goodSelected(unifiedGood: IObject): void {
    if (unifiedGood) {
      this.part = unifiedGood as UnifiedGood;

      this.serialised =
        this.part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      if (this.serialised) {
        this.object.Quantity = '1';
      }

      this.refreshSerialisedItems(unifiedGood as UnifiedGood);
    }
  }

  public serialisedItemSelected(serialisedItem: IObject): void {
    this.serialisedItem = this.part.SerialisedItems?.find(
      (v) => v === serialisedItem
    );
    this.object.Quantity = '1';
  }

  public partSelected(part: IObject): void {
    if (part) {
      this.unifiedGood = this.object.Part.strategy.cls === this.m.UnifiedGood;
      this.nonUnifiedPart =
        this.object.Part.strategy.cls === this.m.NonUnifiedPart;

      this.serialised =
        this.part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      if (this.serialised) {
        this.object.Quantity = '1';
      }

      this.updateFromPart(part as Part);
    }
  }

  public override save(): void {
    this.onSave();

    super.save();
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
          v.Supplier === this.invoice.BilledFrom
      );

      this.serialisedItems = loaded.collection<SerialisedItem>(
        m.Part.SerialisedItems
      );

      if (this.object.SerialisedItem) {
        this.serialisedItems.push(this.object.SerialisedItem);
      }
    });
  }

  private onSave() {
    if (
      this.object.InvoiceItemType !== this.partItemType &&
      this.object.InvoiceItemType !== this.partItemType
    ) {
      this.object.Quantity = '1';
    }
  }
}
