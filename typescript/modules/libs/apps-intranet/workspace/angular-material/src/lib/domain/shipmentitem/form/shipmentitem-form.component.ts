import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  Pull,
  IPullResult,
  IObject,
  And,
} from '@allors/system/workspace/domain';
import {
  Facility,
  Good,
  InventoryItem,
  NonSerialisedInventoryItem,
  OrderShipment,
  Part,
  Product,
  PurchaseOrderItem,
  QuoteItemState,
  QuoteState,
  RequestItemState,
  RequestState,
  SalesOrderItem,
  SalesOrderItemState,
  SalesOrderState,
  SerialisedInventoryItem,
  SerialisedItem,
  SerialisedItemAvailability,
  Shipment,
  ShipmentItem,
  ShipmentItemState,
  ShipmentState,
  SupplierOffering,
  UnifiedGood,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { MatSnackBar } from '@angular/material/snack-bar';
import { Filters } from '../../../filters/filters';
import { ShipmentWhereShipmentItem } from '../../../../../../../../extranet/workspace/meta/src/lib/generated/m.g';

@Component({
  templateUrl: './shipmentitem-form.component.html',
  providers: [ContextService],
})
export class ShipmentItemFormComponent extends AllorsFormComponent<ShipmentItem> {
  readonly m: M;
  shipment: Shipment;
  inventoryItems: InventoryItem[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  serialisedItems: SerialisedItem[] = [];
  sold: SerialisedItemAvailability;
  orderShipment: OrderShipment;
  salesOrderItems: SalesOrderItem[] = [];
  selectedSalesOrderItem: SalesOrderItem;
  selectedPurchaseOrderItem: PurchaseOrderItem;
  purchaseOrderItems: PurchaseOrderItem[] = [];
  supplierOfferings: SupplierOffering[];
  isCustomerShipment: boolean;
  isPurchaseShipment: boolean;
  isPurchaseReturn: boolean;
  isSerialized: boolean;
  supplierPartsFilter: SearchFactory;
  serialisedItemAvailabilities: SerialisedItemAvailability[];
  selectedFacility: Facility;
  addFacility = false;
  facilities: Facility[];
  goodIsSelected = false;
  partIsSelected = false;

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

  private previousGood;
  private previousPart;

  unifiedGoodsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    public snackBar: MatSnackBar
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;

    this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(this.m, treeBuilder);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.SerialisedItemAvailability({}),
      p.SerialisedInventoryItemState({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedInventoryItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
      }),
      p.RequestItemState({}),
      p.RequestState({}),
      p.QuoteItemState({}),
      p.QuoteState({}),
      p.SalesOrderItemState({}),
      p.SalesOrderState({}),
      p.PurchaseOrderItemState({}),
      p.ShipmentItemState({}),
      p.ShipmentState({}),
      p.Facility({})
    );

    if (this.editRequest) {
      pulls.push(
        p.ShipmentItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Good: {
              UnifiedGood_InventoryItemKind: {},
            },
            Part: {
              InventoryItemKind: {},
            },
            SerialisedItem: {},
            ShipmentItemState: {},
            StoredInFacility: {},
            ShipmentWhereShipmentItem: {},
          },
        }),
        p.ShipmentItem({
          objectId: this.editRequest.objectId,
          select: {
            OrderShipmentsWhereShipmentItem: {
              include: {
                OrderItem: {},
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Shipment({
          objectId: initializer.id,
          include: {
            ShipmentItems: {},
            ShipToAddress: {},
          },
        }),
        p.Shipment({
          objectId: initializer.id,
          select: {
            ShipToAddress: {
              SalesOrderItemsWhereDerivedShipToAddress: {
                include: {
                  Product: {},
                  SerialisedItem: {},
                  SalesOrderWhereSalesOrderItem: {
                    SalesOrderState: {},
                  },
                },
              },
            },
          },
        }),
        p.Shipment({
          objectId: initializer.id,
          select: {
            ShipToParty: {
              Organisation_PurchaseOrdersWhereTakenViaSupplier: {
                PurchaseOrderItems: {
                  include: {
                    Part: {},
                    SerialisedItem: {},
                    PurchaseOrderItemState: {},
                    PurchaseOrderWherePurchaseOrderItem: {
                      PurchaseOrderState: {},
                    },
                  },
                },
              },
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

    if (this.createRequest) {
      this.shipment = pullResult.object<Shipment>(this.m.Shipment);
    } else {
      this.shipment = this.object.ShipmentWhereShipmentItem;
    }

    this.isCustomerShipment =
      this.shipment.strategy.cls === this.m.CustomerShipment;
    this.isPurchaseShipment =
      this.shipment.strategy.cls === this.m.PurchaseShipment;
    this.isPurchaseReturn =
      this.shipment.strategy.cls === this.m.PurchaseReturn;

    this.facilities = pullResult.collection<Facility>(this.m.Facility);
    this.serialisedItemAvailabilities =
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );
    this.sold = this.serialisedItemAvailabilities?.find(
      (v: SerialisedItemAvailability) =>
        v.UniqueId === '9bdc0a55-4e3c-4604-b054-2441a551aa1c'
    );

    const salesOrderStates = pullResult.collection<SalesOrderState>(
      this.m.SalesOrderState
    );
    const inProcess = salesOrderStates?.find(
      (v) => v.UniqueId === 'ddbb678e-9a66-4842-87fd-4e628cff0a75'
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

    const salesOrderItems = pullResult.collection<SalesOrderItem>(
      this.m.SalesOrderItem
    );
    if (salesOrderItems) {
      this.salesOrderItems = salesOrderItems?.filter(
        (v) =>
          v.SalesOrderWhereSalesOrderItem.SalesOrderState === inProcess &&
          parseFloat(v.QuantityRequestsShipping) > 0
      );
    }

    const purchaseOrderItems = pullResult.collection<PurchaseOrderItem>(
      this.m.PurchaseOrderItem
    );
    if (purchaseOrderItems) {
      this.purchaseOrderItems = purchaseOrderItems?.filter(
        (v) =>
          Number(v.QuantityReceived) > 0 &&
          Number(v.QuantityReceived) > Number(v.QuantityReturned)
      );
    }

    if (this.isPurchaseShipment) {
      this.supplierPartsFilter = new SearchFactory({
        objectType: this.m.Part,
        roleTypes: [this.m.Part.Name, this.m.Part.SearchString],
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
                object: this.shipment.ShipFromParty,
              },
            },
          });
        },
      });
    }

    if (this.isPurchaseReturn) {
      this.supplierPartsFilter = new SearchFactory({
        objectType: this.m.Part,
        roleTypes: [this.m.Part.Name, this.m.Part.SearchString],
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
                object: this.shipment.ShipToParty,
              },
            },
          });
        },
      });
    }

    if (this.createRequest) {
      this.selectedFacility = this.shipment.ShipToFacility;
      this.shipment.addShipmentItem(this.object);
    } else {
      // This UI does not allow shipmentitem to be combination from multiple order items
      const orderShipments = pullResult.collection<OrderShipment>(
        this.m.ShipmentItem.OrderShipmentsWhereShipmentItem
      );

      if (orderShipments && orderShipments.length > 0) {
        this.orderShipment = orderShipments[0];
        if (this.orderShipment.OrderItem) {
          if (this.isCustomerShipment) {
            this.selectedSalesOrderItem = this.orderShipment
              .OrderItem as SalesOrderItem;
            this.salesOrderItems.push(this.selectedSalesOrderItem);
          }

          if (this.isPurchaseShipment || this.isPurchaseReturn) {
            this.selectedPurchaseOrderItem = this.orderShipment
              .OrderItem as PurchaseOrderItem;
            this.purchaseOrderItems.push(this.selectedPurchaseOrderItem);
          }
        }
      }

      if (this.object.Good) {
        this.goodIsSelected = true;
        this.partIsSelected = false;
        this.previousGood = this.object.Good;
        this.loadProduct(this.object.Good);
      }

      if (this.object.Part) {
        this.partIsSelected = true;
        this.goodIsSelected = false;
        this.previousPart = this.object.Part;
        this.loadPart(this.object.Part);
      }

      this.selectedFacility = this.object.StoredInFacility;
      this.serialisedItems.push(this.object.SerialisedItem);
    }
  }

  public override save(): void {
    this.onSave();

    super.save();
  }

  public salesOrderItemSelected(obj: IObject): void {
    if (obj) {
      const salesOrderItem = obj as SalesOrderItem;
      this.object.Good = salesOrderItem.Product as Good;
      this.object.Quantity = salesOrderItem.QuantityRequestsShipping;
      this.object.SerialisedItem = salesOrderItem.SerialisedItem;
    }
  }

  public purchaseOrderItemSelected(obj: IObject): void {
    if (obj) {
      const purchaseOrderItem = obj as PurchaseOrderItem;
      this.object.Part = purchaseOrderItem.Part as Part;
      this.object.Quantity = (
        Number(purchaseOrderItem.QuantityReceived) -
        Number(purchaseOrderItem.QuantityReturned)
      ).toString();
      this.object.SerialisedItem = purchaseOrderItem.SerialisedItem;
    }
  }

  public goodSelected(product: IObject): void {
    this.goodIsSelected = true;
    this.partIsSelected = false;

    if (product) {
      this.loadProduct(product as Product);
    }
  }

  public partSelected(part: IObject): void {
    this.partIsSelected = true;
    this.goodIsSelected = false;

    if (part) {
      this.loadPart(part as Part);
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

      const onQuoteItem = serialisedItem.QuoteItemsWhereSerialisedItem?.find(
        (v) =>
          (v.QuoteItemState === this.draftQuoteItem ||
            v.QuoteItemState === this.submittedQuoteItem ||
            v.QuoteItemState === this.approvedQuoteItem ||
            v.QuoteItemState === this.awaitingApprovalQuoteItem ||
            v.QuoteItemState === this.awaitingAcceptanceQuoteItem ||
            v.QuoteItemState === this.acceptedQuoteItem) &&
          (v.QuoteWhereQuoteItem.QuoteState === this.createdQuote ||
            v.QuoteWhereQuoteItem.QuoteState === this.approvedQuote ||
            v.QuoteWhereQuoteItem.QuoteState === this.awaitingAcceptanceQuote ||
            v.QuoteWhereQuoteItem.QuoteState === this.acceptedQuote)
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

      const onOtherShipmentItem =
        serialisedItem.ShipmentItemsWhereSerialisedItem?.find(
          (v) =>
            (v.ShipmentItemState === this.createdShipmentItem ||
              v.ShipmentItemState === this.pickingShipmentItem ||
              v.ShipmentItemState === this.pickedShipmentItem ||
              v.ShipmentItemState === this.packedShipmentItem) &&
            (v.ShipmentWhereShipmentItem?.ShipmentState ===
              this.createdShipment ||
              v.ShipmentWhereShipmentItem?.ShipmentState ===
                this.pickingShipment ||
              v.ShipmentWhereShipmentItem?.ShipmentState ===
                this.pickingShipment ||
              v.ShipmentWhereShipmentItem?.ShipmentState ===
                this.packedShipment ||
              v.ShipmentWhereShipmentItem?.ShipmentState ===
                this.onholdShipment)
        );

      if (onRequestItem) {
        this.snackBar.open(
          `Item already requested with ${onRequestItem.RequestWhereRequestItem.RequestNumber}`,
          'close'
        );
      }

      if (onQuoteItem) {
        this.snackBar.open(
          `Item already quoted with ${onQuoteItem.QuoteWhereQuoteItem.QuoteNumber}`,
          'close'
        );
      }

      if (onOrderItem) {
        this.snackBar.open(
          `Item already ordered with ${onOrderItem.SalesOrderWhereSalesOrderItem.OrderNumber}`,
          'close'
        );
      }

      if (onOtherShipmentItem) {
        this.snackBar.open(
          `Item already shipped with ${onOtherShipmentItem.ShipmentWhereShipmentItem.ShipmentNumber}`,
          'close'
        );
      }

      this.object.Quantity = '1';
    }
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.selectedFacility = facility;
  }

  private loadProduct(product: Product): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedGood({
        objectId: product.id,
        select: {
          Part: {
            include: {
              InventoryItemKind: x,
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
          InventoryItemKind: x,
          SerialisedItems: {
            SerialisedInventoryItemsWhereSerialisedItem: x,
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
      const part =
        pullResult.object<UnifiedGood>(this.m.UnifiedGood) ||
        pullResult.object<Part>(this.m.Part);

      this.isSerialized =
        part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      if (this.isCustomerShipment) {
        this.serialisedItems = part.SerialisedItems.filter(
          (v) => v.AvailableForSale === true
        );
      } else {
        this.serialisedItems = part.SerialisedItems.filter(
          (v) => v.SerialisedInventoryItemsWhereSerialisedItem.length === 0
        );
      }

      if (this.object.Good !== this.previousGood) {
        this.object.SerialisedItem = null;
        this.previousGood = this.object.Good;
      }
    });
  }

  private loadPart(part: Part): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedPart({
        objectId: part.id,
        select: {
          InventoryItemsWherePart: {
            include: {
              Facility: x,
            },
          },
        },
      }),
      pull.NonUnifiedPart({
        objectId: part.id,
        include: {
          InventoryItemKind: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      this.isSerialized =
        part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
      this.inventoryItems = pullResult.collection<InventoryItem>(
        this.m.InventoryItem
      );

      if (this.object.Part !== this.previousPart) {
        this.object.SerialisedItem = null;
        this.previousPart = this.object.Part;
      }
    });
  }

  private onSave() {
    if (this.selectedSalesOrderItem || this.selectedPurchaseOrderItem) {
      if (this.orderShipment == null) {
        this.orderShipment = this.allors.context.create<OrderShipment>(
          this.m.OrderShipment
        );
      }

      this.orderShipment.OrderItem =
        this.selectedSalesOrderItem ?? this.selectedPurchaseOrderItem;
      this.orderShipment.ShipmentItem = this.object;
      this.orderShipment.Quantity = this.object.Quantity;
    }

    if (this.isPurchaseShipment || this.isPurchaseReturn) {
      this.object.StoredInFacility = this.selectedFacility;
    }
  }
}
