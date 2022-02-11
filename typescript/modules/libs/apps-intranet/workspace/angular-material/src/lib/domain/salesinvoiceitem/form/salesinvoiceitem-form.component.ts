import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Facility,
  InternalOrganisation,
  InventoryItem,
  InvoiceItemType,
  IrpfRegime,
  NonSerialisedInventoryItem,
  NonUnifiedPart,
  Part,
  Product,
  SalesInvoice,
  SalesInvoiceItem,
  SalesOrderItem,
  SerialisedInventoryItem,
  SerialisedItem,
  SerialisedItemAvailability,
  SupplierOffering,
  UnifiedGood,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './salesinvoiceitem-form.component.html',
  providers: [ContextService],
})
export class SalesInvoiceItemFormComponent extends AllorsFormComponent<SalesInvoiceItem> {
  readonly m: M;

  invoice: SalesInvoice;
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
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.goodsFacilityFilter = new SearchFactory({
      objectType: this.m.Good,
      roleTypes: [this.m.Good.Name],
    });

    this.goodsFilter = Filters.goodsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.warehouses,
      p.SerialisedItemAvailability({}),
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
      }),
      p.SerialisedItemAvailability({})
    );

    if (this.editRequest) {
      pulls.push(
        p.SalesInvoiceItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            SalesInvoiceItemState: {},
            SerialisedItem: {},
            NextSerialisedItemAvailability: {},
            Facility: {
              Owner: {},
            },
            DerivedVatRegime: {
              VatRates: {},
            },
            DerivedIrpfRegime: {
              IrpfRates: {},
            },
          },
        }),
        p.SalesInvoiceItem({
          objectId: this.editRequest.objectId,
          select: {
            SalesInvoiceWhereSalesInvoiceItem: {
              include: {
                SalesInvoiceItems: {},
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

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.SalesInvoice({
          objectId: initializer.id,
          include: {
            SalesInvoiceItems: {},
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
    this.orderItem = pullResult.object<SalesOrderItem>(this.m.SalesOrderItem);
    this.parts = pullResult.collection<NonUnifiedPart>(this.m.NonUnifiedPart);
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.serialisedItemAvailabilities =
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );
    this.facilities = this.fetcher.getWarehouses(pullResult);
    this.invoiceItemTypes = pullResult.collection<InvoiceItemType>(
      this.m.InvoiceItemType
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
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );
    this.inRent = serialisedItemAvailabilities?.find(
      (v: SerialisedItemAvailability) =>
        v.UniqueId === 'ec87f723-2284-4f5c-ba57-fcf328a0b738'
    );

    if (this.createRequest) {
      this.invoice = pullResult.object<SalesInvoice>(this.m.SalesInvoice);
      this.invoice.addSalesInvoiceItem(this.object);
      this.vatRegimeInitialRole = this.invoice.DerivedVatRegime;
      this.irpfRegimeInitialRole = this.invoice.DerivedIrpfRegime;
    } else {
      this.invoice = this.object.SalesInvoiceWhereSalesInvoiceItem;

      this.previousProduct = this.object.Product;
      this.serialisedItem = this.object.SerialisedItem;

      if (this.object.InvoiceItemType === this.productItemType) {
        this.goodSelected(this.object.Product);
      }
    }
  }
  public goodSelected(object: any) {
    if (object) {
      this.refreshSerialisedItems(object as Product);
    }
  }

  public serialisedItemSelected(serialisedItem: IObject): void {
    const unifiedGood = this.object.Product as UnifiedGood;
    this.serialisedItem = unifiedGood.SerialisedItems?.find(
      (v) => v === serialisedItem
    );
    this.object.AssignedUnitPrice = this.serialisedItem.ExpectedSalesPrice;
    this.object.Quantity = '1';
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

      if (this.object.Product !== this.previousProduct) {
        this.object.SerialisedItem = null;
        this.serialisedItem = null;
        this.previousProduct = this.object.Product;
      }
    });
  }
}
