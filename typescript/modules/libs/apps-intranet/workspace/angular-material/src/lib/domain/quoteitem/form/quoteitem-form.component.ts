import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  InternalOrganisation,
  InventoryItem,
  InvoiceItemType,
  IrpfRegime,
  NonSerialisedInventoryItem,
  Part,
  Product,
  Quote,
  QuoteItem,
  QuoteItemState,
  QuoteState,
  RequestItem,
  RequestItemState,
  RequestState,
  SalesOrderItemState,
  SalesOrderState,
  SerialisedInventoryItem,
  SerialisedItem,
  ShipmentItemState,
  ShipmentState,
  UnifiedGood,
  UnifiedProduct,
  UnitOfMeasure,
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
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  templateUrl: './quoteitem-form.component.html',
  providers: [ContextService],
})
export class QuoteItemFormComponent extends AllorsFormComponent<QuoteItem> {
  readonly m: M;
  quote: Quote;
  invoiceItemTypes: InvoiceItemType[];
  partItemType: InvoiceItemType;
  productItemType: InvoiceItemType;
  requestItem: RequestItem;
  inventoryItems: InventoryItem[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  unitsOfMeasure: UnitOfMeasure[];
  goodsFilter: SearchFactory;
  partsFilter: SearchFactory;
  part: Part;
  parts: Part[];
  serialisedItem: SerialisedItem;
  serialisedItems: SerialisedItem[] = [];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  vatRegimeInitialRole: VatRegime;
  irpfRegimeInitialRole: IrpfRegime;

  private previousProduct;
  private previousSerialisedItem: SerialisedItem;

  draftRequestItem: RequestItemState;
  submittedRequestItem: RequestItemState;
  anonymousRequest: RequestState;
  submittedRequest: RequestState;
  pendingCustomerRequest: RequestState;
  draftQuoteItem: QuoteItemState;
  submittedQuoteItem: QuoteItemState;
  approvedQuoteItem: QuoteItemState;
  awaitingApprovalQuoteItem: QuoteItemState;
  awaitingAcceptanceQuoteItem: QuoteItemState;
  acceptedQuoteItem: QuoteItemState;
  createdQuote: QuoteState;
  approvedQuote: QuoteState;
  acceptedQuote: QuoteState;
  awaitingAcceptanceQuote: QuoteState;
  provisionalOrderItem: SalesOrderItemState;
  requestsApprovalOrderItem: SalesOrderItemState;
  readyForPostingOrderItem: SalesOrderItemState;
  awaitingAcceptanceOrderItem: SalesOrderItemState;
  onHoldOrderItem: SalesOrderItemState;
  inProcessOrderItem: SalesOrderItemState;
  provisionalOrder: SalesOrderState;
  readyForPostingOrder: SalesOrderState;
  requestsApprovalOrder: SalesOrderState;
  awaitingAcceptanceOrder: SalesOrderState;
  inProcessOrder: SalesOrderState;
  onHoldOrder: SalesOrderState;
  createdShipmentItem: ShipmentItemState;
  pickingShipmentItem: ShipmentItemState;
  pickedShipmentItem: ShipmentItemState;
  packedShipmentItem: ShipmentItemState;
  createdShipment: ShipmentState;
  pickingShipment: ShipmentState;
  pickedShipment: ShipmentState;
  packedShipment: ShipmentState;
  onholdShipment: ShipmentState;
  internalOrganisation: InternalOrganisation;
  showIrpf: boolean;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,

    private fetcher: FetcherService,
    public snackBar: MatSnackBar
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.goodsFilter = Filters.goodsFilter(this.m);
    this.partsFilter = Filters.nonUnifiedPartsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.IrpfRegime({
        sorting: [{ roleType: m.IrpfRegime.Name }],
      }),
      p.InvoiceItemType({
        predicate: {
          kind: 'Equals',
          propertyType: m.InvoiceItemType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.InvoiceItemType.Name }],
      }),
      p.UnitOfMeasure({
        predicate: {
          kind: 'Equals',
          propertyType: m.UnitOfMeasure.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.UnitOfMeasure.Name }],
      }),
      p.RequestItemState({}),
      p.RequestState({}),
      p.QuoteItemState({}),
      p.QuoteState({}),
      p.SalesOrderItemState({}),
      p.SalesOrderState({}),
      p.ShipmentItemState({}),
      p.ShipmentState({})
    );

    if (this.editRequest) {
      pulls.push(
        p.QuoteItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            QuoteItemState: {},
            RequestItem: {},
            Product: {},
            SerialisedItem: {},
            DerivedVatRegime: {
              VatRates: {},
            },
            DerivedIrpfRegime: {
              IrpfRates: {},
            },
          },
        }),
        p.QuoteItem({
          objectId: this.editRequest.objectId,
          select: {
            RequestItem: {},
          },
        }),
        p.QuoteItem({
          objectId: this.editRequest.objectId,
          select: {
            QuoteWhereQuoteItem: {
              include: {
                QuoteItems: {},
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
        p.ProductQuote({
          objectId: initializer.id,
          include: {
            QuoteItems: {},
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
    this.requestItem = pullResult.object<RequestItem>(
      this.m.QuoteItem.RequestItem
    );
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    const piece = this.unitsOfMeasure?.find(
      (v: UnitOfMeasure) =>
        v.UniqueId === 'f4bbdb52-3441-4768-92d4-729c6c5d6f1b'
    );

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

    const requestItemStates = pullResult.collection<RequestItemState>(
      this.m.RequestItemState
    );
    this.draftRequestItem = requestItemStates?.find(
      (v: RequestItemState) =>
        v.UniqueId === 'b173dfbe-9421-4697-8ffb-e46afc724490'
    );
    this.submittedRequestItem = requestItemStates?.find(
      (v: RequestItemState) =>
        v.UniqueId === 'b118c185-de34-4131-be1f-e6162c1dea4b'
    );

    const requestStates = pullResult.collection<RequestState>(
      this.m.RequestState
    );
    this.anonymousRequest = requestStates?.find(
      (v: RequestState) => v.UniqueId === '2f054949-e30c-4954-9a3c-191559de8315'
    );
    this.submittedRequest = requestStates?.find(
      (v: RequestState) => v.UniqueId === 'db03407d-bcb1-433a-b4e9-26cea9a71bfd'
    );
    this.pendingCustomerRequest = requestStates?.find(
      (v: RequestState) => v.UniqueId === '671fda2f-5aa6-4ea5-b5d6-c914f0911690'
    );

    const quoteItemStates = pullResult.collection<QuoteItemState>(
      this.m.QuoteItemState
    );
    this.draftQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === '84ad17a3-10f7-4fdb-b76a-41bdb1edb0e6'
    );
    this.submittedQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === 'e511ea2d-6eb9-428d-a982-b097938a8ff8'
    );
    this.approvedQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === '3335810c-9e26-4604-b272-d18b831e79e0'
    );
    this.awaitingApprovalQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === '76155bb7-53a3-4175-b026-74274a337820'
    );
    this.awaitingAcceptanceQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === 'e0982b61-deb1-47cb-851b-c182f03326a1'
    );
    this.acceptedQuoteItem = quoteItemStates?.find(
      (v: QuoteItemState) =>
        v.UniqueId === '6e56c9f1-7bea-4ced-a724-67e4221a5993'
    );

    const quoteStates = pullResult.collection<QuoteState>(this.m.QuoteState);
    this.createdQuote = quoteStates?.find(
      (v: QuoteState) => v.UniqueId === 'b1565cd4-d01a-4623-bf19-8c816df96aa6'
    );
    this.approvedQuote = quoteStates?.find(
      (v: QuoteState) => v.UniqueId === '675d6899-1ebb-4fdb-9dc9-b8aef0a135d2'
    );
    this.awaitingAcceptanceQuote = quoteStates?.find(
      (v: QuoteState) => v.UniqueId === '324beb70-937f-4c4d-a7e9-2e3063c88a62'
    );
    this.acceptedQuote = quoteStates?.find(
      (v: QuoteState) => v.UniqueId === '3943f87c-f098-49c8-89ba-12047c826777'
    );

    const salesOrderItemStates = pullResult.collection<SalesOrderItemState>(
      this.m.SalesOrderItemState
    );
    this.provisionalOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === '5b0993b5-5784-4e8d-b1ad-93affac9a913'
    );
    this.readyForPostingOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === '6e4f9535-a7ce-483f-9fbd-c9fd331d355e'
    );
    this.requestsApprovalOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === '8d3a4a0a-ed27-4478-baff-ece591068712'
    );
    this.awaitingAcceptanceOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === 'd3965e9b-764d-4787-87b4-82cb2acb0878'
    );
    this.inProcessOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === 'e08401f7-1deb-4b27-b0c5-8f034bffedb5'
    );
    this.onHoldOrderItem = salesOrderItemStates?.find(
      (v: SalesOrderItemState) =>
        v.UniqueId === '3b185d51-af4a-441e-be0d-f91cfcbdb5d8'
    );

    const salesOrderStates = pullResult.collection<SalesOrderState>(
      this.m.SalesOrderState
    );
    this.provisionalOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === '29abc67d-4be1-4af3-b993-64e9e36c3e6b'
    );
    this.readyForPostingOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === 'e8e7c70b-e920-4f70-96d4-a689518f602c'
    );
    this.requestsApprovalOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === '6b6f6e25-4da1-455d-9c9f-21f2d4316d66'
    );
    this.awaitingAcceptanceOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === '37d344e7-5962-425c-86a9-24bf1e098448'
    );
    this.inProcessOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === 'ddbb678e-9a66-4842-87fd-4e628cff0a75'
    );
    this.onHoldOrder = salesOrderStates?.find(
      (v: SalesOrderState) =>
        v.UniqueId === 'f625fb7e-893e-4f68-ab7b-2bc29a644e5b'
    );

    const shipmentItemStates = pullResult.collection<ShipmentItemState>(
      this.m.ShipmentItemState
    );
    this.createdShipmentItem = shipmentItemStates?.find(
      (v: ShipmentItemState) =>
        v.UniqueId === 'e05818b1-2660-4879-b5a8-8ca96f324f7b'
    );
    this.pickingShipmentItem = shipmentItemStates?.find(
      (v: ShipmentItemState) =>
        v.UniqueId === 'f9043add-e106-4646-8b02-6b10efbb2e87'
    );
    this.pickedShipmentItem = shipmentItemStates?.find(
      (v: ShipmentItemState) =>
        v.UniqueId === 'a8e2014f-c4cb-4a6f-8ccf-0875e439d1f3'
    );
    this.packedShipmentItem = shipmentItemStates?.find(
      (v: ShipmentItemState) =>
        v.UniqueId === '91853258-c875-4f85-bd84-ef1ebd2e5930'
    );

    const shipmentStates = pullResult.collection<ShipmentState>(
      this.m.ShipmentState
    );
    this.createdShipment = shipmentStates?.find(
      (v: ShipmentState) =>
        v.UniqueId === '854ad6a0-b2d1-4b92-8c3d-e9e72dd19afd'
    );
    this.pickingShipment = shipmentStates?.find(
      (v: ShipmentState) =>
        v.UniqueId === '1d76de65-4de4-494d-8677-653b4d62aa42'
    );
    this.pickedShipment = shipmentStates?.find(
      (v: ShipmentState) =>
        v.UniqueId === 'c63c5d25-f139-490f-86d1-2e9e51f5c0a5'
    );
    this.packedShipment = shipmentStates?.find(
      (v: ShipmentState) =>
        v.UniqueId === 'dcabe845-a6f2-49d9-bbae-06fb47012a21'
    );
    this.onholdShipment = shipmentStates?.find(
      (v: ShipmentState) =>
        v.UniqueId === '268cb9a7-6965-47e8-af89-8f915242c23d'
    );

    if (this.createRequest) {
      this.quote = pullResult.object<Quote>(this.m.Quote);
      this.object.UnitOfMeasure = piece;
      this.quote.addQuoteItem(this.object);
      this.vatRegimeInitialRole = this.quote.DerivedVatRegime;
      this.irpfRegimeInitialRole = this.quote.DerivedIrpfRegime;
    } else {
      this.quote = this.object.QuoteWhereQuoteItem as Quote;

      if (this.object.Product) {
        this.previousProduct = this.object.Product;
        this.previousSerialisedItem = this.object.SerialisedItem;
        this.refreshSerialisedItems(this.object.Product);
      } else {
        this.serialisedItems.push(this.object.SerialisedItem);
      }
    }
  }

  public goodSelected(product: IObject): void {
    if (product) {
      this.refreshSerialisedItems(product as Product);
    }
  }

  public serialisedItemSelected(obj: IObject): void {
    if (obj) {
      const serialisedItem = obj as SerialisedItem;

      const onRequestItem =
        serialisedItem.RequestItemsWhereSerialisedItem?.find(
          (v) =>
            (v.RequestItemState === this.draftRequestItem ||
              v.RequestItemState === this.submittedRequestItem) &&
            (v.RequestWhereRequestItem.RequestState === this.anonymousRequest ||
              v.RequestWhereRequestItem.RequestState ===
                this.submittedRequest ||
              v.RequestWhereRequestItem.RequestState ===
                this.pendingCustomerRequest)
        );

      const onOtherQuoteItem =
        serialisedItem.QuoteItemsWhereSerialisedItem?.find(
          (v) =>
            v.QuoteWhereQuoteItem !== this.object.QuoteWhereQuoteItem &&
            (v.QuoteItemState === this.draftQuoteItem ||
              v.QuoteItemState === this.submittedQuoteItem ||
              v.QuoteItemState === this.approvedQuoteItem ||
              v.QuoteItemState === this.awaitingApprovalQuoteItem ||
              v.QuoteItemState === this.awaitingAcceptanceQuoteItem ||
              v.QuoteItemState === this.acceptedQuoteItem) &&
            (v.QuoteWhereQuoteItem?.QuoteState === this.createdQuote ||
              v.QuoteWhereQuoteItem?.QuoteState === this.approvedQuote ||
              v.QuoteWhereQuoteItem?.QuoteState ===
                this.awaitingAcceptanceQuote ||
              v.QuoteWhereQuoteItem?.QuoteState === this.acceptedQuote)
        );

      const onOrderItem =
        serialisedItem.SalesOrderItemsWhereSerialisedItem?.find(
          (v) =>
            (v.SalesOrderItemState === this.provisionalOrderItem ||
              v.SalesOrderItemState === this.readyForPostingOrderItem ||
              v.SalesOrderItemState === this.requestsApprovalOrderItem ||
              v.SalesOrderItemState === this.awaitingAcceptanceOrderItem ||
              v.SalesOrderItemState === this.onHoldOrderItem ||
              v.SalesOrderItemState === this.inProcessOrderItem) &&
            (v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
              this.provisionalOrder ||
              v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
                this.readyForPostingOrder ||
              v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
                this.requestsApprovalOrder ||
              v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
                this.awaitingAcceptanceOrder ||
              v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
                this.onHoldOrder ||
              v.SalesOrderWhereSalesOrderItem.SalesOrderState ===
                this.inProcessOrder)
        );

      const onShipmentItem =
        serialisedItem.ShipmentItemsWhereSerialisedItem?.find(
          (v) =>
            (v.ShipmentItemState === this.createdShipmentItem ||
              v.ShipmentItemState === this.pickingShipmentItem ||
              v.ShipmentItemState === this.pickedShipmentItem ||
              v.ShipmentItemState === this.packedShipmentItem) &&
            (v.ShipmentWhereShipmentItem.ShipmentState ===
              this.createdShipment ||
              v.ShipmentWhereShipmentItem.ShipmentState ===
                this.pickingShipment ||
              v.ShipmentWhereShipmentItem.ShipmentState ===
                this.pickingShipment ||
              v.ShipmentWhereShipmentItem.ShipmentState ===
                this.packedShipment ||
              v.ShipmentWhereShipmentItem.ShipmentState === this.onholdShipment)
        );

      if (onRequestItem) {
        this.snackBar.open(
          `Item already requested with ${onRequestItem.RequestWhereRequestItem.RequestNumber}`,
          'close'
        );
      }

      if (onOtherQuoteItem) {
        this.snackBar.open(
          `Item already quoted with ${onOtherQuoteItem.QuoteWhereQuoteItem.QuoteNumber}`,
          'close'
        );
      }

      if (onOrderItem) {
        this.snackBar.open(
          `Item already ordered with ${onOrderItem.SalesOrderWhereSalesOrderItem.OrderNumber}`,
          'close'
        );
      }

      if (onShipmentItem) {
        this.snackBar.open(
          `Item already shipped with ${onShipmentItem.ShipmentWhereShipmentItem.ShipmentNumber}`,
          'close'
        );
      }

      if (this.object.SerialisedItem !== this.previousSerialisedItem) {
        this.object.AssignedUnitPrice = serialisedItem.ExpectedSalesPrice;
        this.previousSerialisedItem = serialisedItem;
      }

      this.serialisedItem = this.part.SerialisedItems?.find(
        (v) => v === serialisedItem
      );
      this.object.Quantity = '1';
    }
  }

  private refreshSerialisedItems(product: UnifiedProduct): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedGood({
        objectId: product.id,
        select: {
          Part: {
            include: {
              SerialisedItems: {
                RequestItemsWhereSerialisedItem: {
                  RequestItemState: x,
                  RequestWhereRequestItem: {
                    RequestState: x,
                  },
                },
                QuoteItemsWhereSerialisedItem: {
                  QuoteItemState: x,
                  QuoteWhereQuoteItem: {
                    QuoteState: x,
                  },
                },
                SalesOrderItemsWhereSerialisedItem: {
                  SalesOrderItemState: x,
                  SalesOrderWhereSalesOrderItem: {
                    SalesOrderState: x,
                  },
                },
                ShipmentItemsWhereSerialisedItem: {
                  ShipmentItemState: x,
                  ShipmentWhereShipmentItem: {
                    ShipmentState: x,
                  },
                },
              },
            },
          },
        },
      }),
      pull.UnifiedGood({
        objectId: product.id,
        include: {
          SerialisedItems: {
            RequestItemsWhereSerialisedItem: {
              RequestItemState: x,
              RequestWhereRequestItem: {
                RequestState: x,
              },
            },
            QuoteItemsWhereSerialisedItem: {
              QuoteItemState: x,
              QuoteWhereQuoteItem: {
                QuoteState: x,
              },
            },
            SalesOrderItemsWhereSerialisedItem: {
              SalesOrderItemState: x,
              SalesOrderWhereSalesOrderItem: {
                SalesOrderState: x,
              },
            },
            ShipmentItemsWhereSerialisedItem: {
              ShipmentItemState: x,
              ShipmentWhereShipmentItem: {
                ShipmentState: x,
              },
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      this.part =
        pullResult.object<UnifiedGood>(this.m.UnifiedGood) ||
        pullResult.object<Part>(this.m.NonUnifiedGood.Part);
      this.serialisedItems = this.part.SerialisedItems?.filter(
        (v) => v.AvailableForSale === true
      );

      if (this.object.Product !== this.previousProduct) {
        this.object.SerialisedItem = null;
        this.serialisedItem = null;
        this.previousProduct = this.object.Product;
      }
    });
  }
}
