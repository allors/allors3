import { Injectable } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { Class, Composite, RoleType } from '@allors/system/workspace/meta';
import {
  DisplayService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

@Injectable()
export class AppDisplayService implements DisplayService {
  nameByObjectType: Map<Composite, RoleType>;
  descriptionByObjectType: Map<Composite, RoleType>;
  primaryByObjectType: Map<Composite, RoleType[]>;
  secondaryByObjectType: Map<Composite, RoleType[]>;
  tertiaryByObjectType: Map<Composite, RoleType[]>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.nameByObjectType = new Map<Composite, RoleType>([
      [m.Organisation, m.Organisation.Name],
      [m.Person, m.Person.DisplayName],
    ]);

    this.descriptionByObjectType = new Map<Composite, RoleType>([]);

    this.primaryByObjectType = new Map<Composite, RoleType[]>([
      [
        m.CommunicationEvent,
        [
          m.CommunicationEvent.Description,
          m.CommunicationEvent.InvolvedParties,
          m.CommunicationEvent.CommunicationEventState,
          m.CommunicationEvent.EventPurposes,
        ],
      ],
      [
        m.NonSerialisedInventoryItem,
        [
          m.NonSerialisedInventoryItem.Facility,
          m.NonSerialisedInventoryItem.Part,
          m.NonSerialisedInventoryItem.UnitOfMeasure,
          m.NonSerialisedInventoryItem.PartLocation,
          m.NonSerialisedInventoryItem.QuantityOnHand,
          m.NonSerialisedInventoryItem.AvailableToPromise,
        ],
      ],
      [
        m.OrderAdjustment,
        [m.OrderAdjustment.Amount, m.OrderAdjustment.Percentage],
      ],
      [m.Organisation, [m.Organisation.Name]],
      [
        m.PartyContactMechanism,
        [
          m.PartyContactMechanism.Party,
          m.PartyContactMechanism.ContactPurposes,
          m.PartyContactMechanism.ContactMechanism,
        ],
      ],
      [
        m.PartyRate,
        [m.PartyRate.RateType, m.PartyRate.Rate, m.PartyRate.Frequency],
      ],
      [m.PartyRelationship, [m.PartyRelationship.Parties]],
      [
        m.Person,
        [m.Person.FirstName, m.Person.LastName, m.Person.DisplayEmail],
      ],
      [m.PriceComponent, [m.PriceComponent.Price]],
      [m.ProductIdentification, [m.ProductIdentification.Identification]],
      [
        m.ProductQuote,
        [
          m.ProductQuote.QuoteNumber,
          m.ProductQuote.Receiver,
          m.ProductQuote.QuoteState,
        ],
      ],
      [
        m.PurchaseOrder,
        [
          m.PurchaseOrder.OrderNumber,
          m.PurchaseOrder.Description,
          m.PurchaseOrder.CustomerReference,
          m.PurchaseOrder.TotalExVat,
          m.PurchaseOrder.PurchaseOrderState,
          m.PurchaseOrder.PurchaseOrderShipmentState,
          m.PurchaseOrder.PurchaseOrderPaymentState,
        ],
      ],
      [
        m.QuoteItem,
        [
          m.QuoteItem.Product,
          m.QuoteItem.SerialisedItem,
          m.QuoteItem.QuoteItemState,
          m.QuoteItem.Quantity,
          m.QuoteItem.UnitPrice,
          m.QuoteItem.TotalExVat,
        ],
      ],
      [
        m.RepeatingPurchaseInvoice,
        [
          m.RepeatingPurchaseInvoice.InternalOrganisation,
          m.RepeatingPurchaseInvoice.Frequency,
          m.RepeatingPurchaseInvoice.DayOfWeek,
          m.RepeatingPurchaseInvoice.PreviousExecutionDate,
          m.RepeatingPurchaseInvoice.NextExecutionDate,
          m.RepeatingPurchaseInvoice.FinalExecutionDate,
        ],
      ],
      [
        m.RequestForQuote,
        [
          m.RequestForQuote.RequestNumber,
          m.RequestForQuote.Originator,
          m.RequestForQuote.RequestState,
        ],
      ],
      [
        m.SalesInvoice,
        [
          m.SalesInvoice.InvoiceNumber,
          m.SalesInvoice.BillToCustomer,
          m.SalesInvoice.TotalExVat,
          m.SalesInvoice.SalesInvoiceState,
        ],
      ],
      [
        m.SalesOrder,
        [
          m.SalesOrder.OrderNumber,
          m.SalesOrder.BillToCustomer,
          m.SalesOrder.TotalExVat,
          m.SalesOrder.SalesOrderState,
        ],
      ],
      [
        m.SerialisedInventoryItem,
        [
          m.SerialisedInventoryItem.Facility,
          m.SerialisedInventoryItem.SerialisedItem,
          m.SerialisedInventoryItem.Part,
          m.SerialisedInventoryItem.Quantity,
          m.SerialisedInventoryItem.SerialisedInventoryItemState,
        ],
      ],
      [
        m.SerialisedItem,
        [
          m.SerialisedItem.ItemNumber,
          m.SerialisedItem.DisplayName,
          m.SerialisedItem.SerialisedItemAvailability,
          m.SerialisedItem.AvailableForSale,
          m.SerialisedItem.Ownership,
          m.SerialisedItem.OwnedBy,
        ],
      ],
      [
        m.ShipmentItem,
        [
          m.ShipmentItem.ShipmentItemState,
          m.ShipmentItem.Good,
          m.ShipmentItem.Part,
        ],
      ],
      [
        m.SupplierOffering,
        [
          m.SupplierOffering.Supplier,
          m.SupplierOffering.Price,
          m.SupplierOffering.UnitOfMeasure,
        ],
      ],
      [
        m.WorkRequirementFulfillment,
        [
          m.WorkRequirementFulfillment.WorkEffortNumber,
          m.WorkRequirementFulfillment.WorkEffortName,
          m.WorkRequirementFulfillment.WorkRequirementNumber,
          m.WorkRequirementFulfillment.WorkRequirementDescription,
        ],
      ],
      [
        m.WorkTask,
        [
          m.WorkTask.WorkEffortNumber,
          m.WorkTask.Customer,
          m.WorkTask.WorkEffortState,
          m.WorkTask.TotalCost,
        ],
      ],
    ]);

    this.secondaryByObjectType = new Map<Composite, RoleType[]>([
      [
        m.WorkRequirementFulfillment,
        [
          m.WorkRequirementFulfillment.FullfillmentOf,
          m.WorkRequirementFulfillment.FullfilledBy,
          m.WorkRequirementFulfillment.FixedAsset,
        ],
      ],
    ]);

    this.tertiaryByObjectType = new Map<Composite, RoleType[]>([]);
  }

  name(objectType: Composite): RoleType {
    return this.nameByObjectType.get(objectType);
  }

  description(objectType: Composite): RoleType {
    return this.nameByObjectType.get(objectType);
  }

  primary(objectType: Composite): RoleType[] {
    return this.primaryByObjectType.get(objectType) ?? [];
  }

  secondary(objectType: Composite): RoleType[] {
    return this.secondaryByObjectType.get(objectType) ?? [];
  }

  tertiary(objectType: Composite): RoleType[] {
    return this.tertiaryByObjectType.get(objectType) ?? [];
  }
}
