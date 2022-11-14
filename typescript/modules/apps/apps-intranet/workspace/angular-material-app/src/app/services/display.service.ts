import { Injectable } from '@angular/core';
import { Composite, RoleType } from '@allors/system/workspace/meta';
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
      [m.AgreementTerm, m.WorkRequirement.Description],
      [m.AgreementTerm, m.WorkRequirement.Description],
      [m.BankAccount, m.BankAccount.NameOnAccount],
      [m.Brand, m.Brand.Name],
      [m.Carrier, m.Carrier.Name],
      [m.Catalogue, m.Catalogue.Name],
      [m.CommunicationEventPurpose, m.CommunicationEventPurpose.Name],
      [m.CommunicationEventState, m.CommunicationEventState.Name],
      [m.ContactMechanismPurpose, m.ContactMechanismPurpose.Name],
      [m.ContactMechanismType, m.ContactMechanismType.Name],
      [m.Country, m.Country.Name],
      [m.Currency, m.Currency.IsoCode],
      [m.CustomerReturn, m.CustomerReturn.ShipmentNumber],
      [m.CustomerShipment, m.CustomerShipment.ShipmentNumber],
      [
        m.CustomOrganisationClassification,
        m.CustomOrganisationClassification.Name,
      ],
      [m.DayOfWeek, m.DayOfWeek.Name],
      [m.DeliverableBasedService, m.DeliverableBasedService.DisplayName],
      [m.Dimension, m.Dimension.Name],
      [m.DropShipment, m.DropShipment.ShipmentNumber],
      [m.EanIdentification, m.EanIdentification.Identification],
      [m.EmailAddress, m.EmailAddress.DisplayName],
      [m.Facility, m.Facility.Name],
      [m.FacilityType, m.FacilityType.Name],
      [m.GenderType, m.GenderType.Name],
      [m.IncoTerm, m.IncoTerm.Description],
      [m.IncoTermType, m.IncoTermType.Name],
      [m.IndustryClassification, m.IndustryClassification.Name],
      [m.InventoryItemKind, m.InventoryItemKind.Name],
      [m.InventoryStrategy, m.InventoryStrategy.Name],
      [m.InventoryTransactionReason, m.InventoryTransactionReason.Name],
      [m.InvoiceItemType, m.InvoiceItemType.Name],
      [m.InvoiceSequence, m.InvoiceSequence.Name],
      [m.InvoiceTerm, m.InvoiceTerm.Description],
      [m.InvoiceTermType, m.InvoiceTermType.Name],
      [m.IsbnIdentification, m.IsbnIdentification.Identification],
      [m.LegalForm, m.LegalForm.Description],
      [m.LegalTerm, m.LegalTerm.Description],
      [m.MaritalStatus, m.MaritalStatus.Name],
      [m.Model, m.Model.Name],
      [m.NonSerialisedInventoryItem, m.NonSerialisedInventoryItem.DisplayName],
      [
        m.NonSerialisedInventoryItemState,
        m.NonSerialisedInventoryItemState.Name,
      ],
      [
        m.NonSerialisedInventoryItemObjectState,
        m.NonSerialisedInventoryItemObjectState.Name,
      ],
      [m.NonUnifiedGood, m.NonUnifiedGood.DisplayName],
      [m.NonUnifiedPart, m.NonUnifiedPart.DisplayName],
      [m.OrderTerm, m.OrderTerm.Description],
      [m.OrderTermType, m.OrderTermType.Name],
      [m.Ordinal, m.Ordinal.Name],
      [m.Organisation, m.Organisation.DisplayName],
      [m.OrganisationContactKind, m.OrganisationContactKind.Description],
      [m.OrganisationRole, m.OrganisationRole.Name],
      [m.OrganisationUnit, m.OrganisationUnit.Name],
      [m.Ownership, m.Ownership.Name],
      [m.PartCategory, m.PartCategory.DisplayName],
      [m.PartNumber, m.PartNumber.Identification],
      [m.PartSpecificationState, m.PartSpecificationState.Name],
      [m.PartSpecificationType, m.PartSpecificationType.Name],
      [m.PartyClassification, m.PartyClassification.Name],
      [m.Person, m.Person.DisplayName],
      [m.PersonalTitle, m.PersonalTitle.Name],
      [m.PickListState, m.PickListState.Name],
      [m.PositionStatus, m.PositionStatus.Name],
      [m.PositionType, m.PositionType.Description],
      [m.PostalAddress, m.PostalAddress.DisplayName],
      [m.Priority, m.Priority.Name],
      [m.ProductCategory, m.ProductCategory.DisplayName],
      [m.ProductIdentification, m.ProductIdentification.Identification],
      [m.ProductIdentificationType, m.ProductIdentificationType.Name],
      [m.ProductNumber, m.ProductNumber.Identification],
      [m.ProductQuote, m.ProductQuote.QuoteNumber],
      [m.ProductType, m.ProductType.Name],
      [m.Proposal, m.Proposal.QuoteNumber],
      [m.PurchaseInvoice, m.PurchaseInvoice.InvoiceNumber],
      [m.PurchaseInvoiceItemState, m.PurchaseInvoiceItemState.Name],
      [m.PurchaseInvoiceState, m.PurchaseInvoiceState.Name],
      [m.PurchaseInvoiceType, m.PurchaseInvoiceType.Name],
      [m.PurchaseOrder, m.PurchaseOrder.DisplayName],
      [m.PurchaseOrderItem, m.PurchaseOrderItem.DisplayName],
      [m.PurchaseOrderItemPaymentState, m.PurchaseOrderItemPaymentState.Name],
      [m.PurchaseOrderItemShipmentState, m.PurchaseOrderItemShipmentState.Name],
      [m.PurchaseOrderItemState, m.PurchaseOrderItemState.Name],
      [m.PurchaseOrderPaymentState, m.PurchaseOrderPaymentState.Name],
      [m.PurchaseOrderShipmentState, m.PurchaseOrderShipmentState.Name],
      [m.PurchaseOrderState, m.PurchaseOrderState.Name],
      [m.PurchaseReturn, m.PurchaseReturn.ShipmentNumber],
      [m.PurchaseShipment, m.PurchaseShipment.ShipmentNumber],
      [m.QuoteItemState, m.QuoteItemState.Name],
      [m.QuoteState, m.QuoteState.Name],
      [m.QuoteTerm, m.QuoteTerm.Description],
      [m.RateType, m.RateType.Name],
      [m.RatingType, m.RatingType.Name],
      [m.RequestForInformation, m.RequestForInformation.RequestNumber],
      [m.RequestForProposal, m.RequestForProposal.RequestNumber],
      [m.RequestForQuote, m.RequestForQuote.RequestNumber],
      [m.RequestItemState, m.RequestItemState.Name],
      [m.RequestState, m.RequestState.Name],
      [m.RequirementSequence, m.RequirementSequence.Name],
      [m.RequirementState, m.RequirementState.Name],
      [m.RequirementType, m.RequirementType.Name],
      [m.SalesChannel, m.SalesChannel.Name],
      [m.SalesInvoice, m.SalesInvoice.InvoiceNumber],
      [m.SalesInvoiceItemState, m.SalesInvoiceItemState.Name],
      [m.SalesInvoiceState, m.SalesInvoiceState.Name],
      [m.SalesInvoiceType, m.SalesInvoiceType.Name],
      [m.SalesOrder, m.SalesOrder.OrderNumber],
      [m.SalesOrderInvoiceState, m.SalesOrderInvoiceState.Name],
      [m.SalesOrderItemInvoiceState, m.SalesOrderItemInvoiceState.Name],
      [m.SalesOrderItemPaymentState, m.SalesOrderItemPaymentState.Name],
      [m.SalesOrderPaymentState, m.SalesOrderPaymentState.Name],
      [m.SalesOrderShipmentState, m.SalesOrderShipmentState.Name],
      [m.SalesOrderItemState, m.SalesOrderItemState.Name],
      [m.SalesOrderState, m.SalesOrderState.Name],
      [m.Salutation, m.Salutation.Name],
      [m.Scope, m.Scope.Name],
      [m.SerialisedInventoryItem, m.SerialisedInventoryItem.DisplayName],
      [m.SerialisedInventoryItemState, m.SerialisedInventoryItemState.Name],
      [m.SerialisedItem, m.SerialisedItem.DisplayName],
      [m.SerialisedItemAvailability, m.SerialisedItemAvailability.Name],
      [
        m.SerialisedItemCharacteristicType,
        m.SerialisedItemCharacteristicType.DisplayName,
      ],
      [m.SerialisedItemState, m.SerialisedItemState.Name],
      [
        m.SerialisedInventoryItemObjectState,
        m.SerialisedInventoryItemObjectState.Name,
      ],
      [m.ShipmentItemState, m.ShipmentItemState.Name],
      [m.ShipmentMethod, m.ShipmentMethod.Name],
      [m.ShipmentState, m.ShipmentState.Name],
      [m.Size, m.Size.Name],
      [m.Skill, m.Skill.Name],
      [m.SkillLevel, m.SkillLevel.Name],
      [m.SkillRating, m.SkillRating.Name],
      [m.SkuIdentification, m.SkuIdentification.Identification],
      [m.StatementOfWork, m.StatementOfWork.QuoteNumber],
      [m.TelecommunicationsNumber, m.TelecommunicationsNumber.DisplayName],
      [m.TimeFrequency, m.TimeFrequency.Name],
      [m.Transfer, m.Transfer.ShipmentNumber],
      [m.UnifiedGood, m.UnifiedGood.DisplayName],
      [m.UnitOfMeasure, m.UnitOfMeasure.Name],
      [m.UpcaIdentification, m.UpcaIdentification.Identification],
      [m.UpceIdentification, m.UpceIdentification.Identification],
      [m.VatClause, m.VatClause.Name],
      [m.Vehicle, m.Vehicle.DisplayName],
      [m.WebAddress, m.WebAddress.DisplayName],
      [m.WorkEffortPurpose, m.WorkEffortPurpose.Name],
      [m.WorkEffortState, m.WorkEffortState.Name],
      [m.WorkEffortType, m.WorkEffortType.Name],
      [m.WorkRequirement, m.WorkRequirement.DisplayName],
      [m.WorkTask, m.WorkTask.WorkEffortNumber],
    ]);

    this.descriptionByObjectType = new Map<Composite, RoleType>([]);

    this.primaryByObjectType = new Map<Composite, RoleType[]>([
      [m.BankAccount, [m.BankAccount.NameOnAccount, m.BankAccount.Iban]],
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
      [m.Payment, [m.Payment.EffectiveDate, m.Payment.Amount]],
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
        m.PurchaseInvoiceItem,
        [
          m.PurchaseInvoiceItem.Part,
          m.PurchaseInvoiceItem.SerialisedItem,
          m.PurchaseInvoiceItem.InvoiceItemType,
          m.PurchaseInvoiceItem.PurchaseInvoiceItemState,
          m.PurchaseInvoiceItem.Quantity,
          m.PurchaseInvoiceItem.TotalExVat,
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
        m.PurchaseOrderItem,
        [
          m.PurchaseOrderItem.Part,
          m.PurchaseOrderItem.SerialisedItem,
          m.PurchaseOrderItem.InvoiceItemType,
          m.PurchaseOrderItem.PurchaseOrderItemShipmentState,
          m.PurchaseOrderItem.QuantityOrdered,
          m.PurchaseOrderItem.QuantityReceived,
          m.PurchaseOrderItem.QuantityReturned,
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
        m.RepeatingSalesInvoice,
        [
          m.RepeatingSalesInvoice.Frequency,
          m.RepeatingSalesInvoice.DayOfWeek,
          m.RepeatingSalesInvoice.PreviousExecutionDate,
          m.RepeatingSalesInvoice.NextExecutionDate,
          m.RepeatingSalesInvoice.FinalExecutionDate,
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
        m.RequestItem,
        [
          m.RequestItem.Product,
          m.RequestItem.SerialisedItem,
          m.RequestItem.RequestItemState,
          m.RequestItem.Quantity,
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
        m.SalesInvoiceItem,
        [
          m.SalesInvoiceItem.Product,
          m.SalesInvoiceItem.SerialisedItem,
          m.SalesInvoiceItem.InvoiceItemType,
          m.SalesInvoiceItem.SalesInvoiceItemState,
          m.SalesInvoiceItem.Quantity,
          m.SalesInvoiceItem.TotalExVat,
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
        m.SalesOrderItem,
        [
          m.SalesOrderItem.Product,
          m.SalesOrderItem.SerialisedItem,
          m.SalesOrderItem.SalesOrderItemState,
          m.SalesOrderItem.QuantityOrdered,
          m.SalesOrderItem.QuantityShipped,
          m.SalesOrderItem.QuantityReserved,
          m.SalesOrderItem.QuantityShortFalled,
          m.SalesOrderItem.QuantityReturned,
        ],
      ],
      [m.SalesTerm, [m.SalesTerm.TermType, m.SalesTerm.TermValue]],
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
          m.ShipmentItem.Quantity,
          m.ShipmentItem.QuantityPicked,
          m.ShipmentItem.QuantityShipped,
        ],
      ],
      [
        m.SupplierOffering,
        [
          m.SupplierOffering.Supplier,
          m.SupplierOffering.Currency,
          m.SupplierOffering.Price,
          m.SupplierOffering.UnitOfMeasure,
        ],
      ],
      [m.TimeEntry, [m.TimeEntry.Worker, m.TimeEntry.AmountOfTime]],
      [
        m.WorkEffort,
        [
          m.WorkEffort.WorkEffortNumber,
          m.WorkEffort.TakenBy,
          m.WorkEffort.Name,
          m.WorkEffort.Description,
        ],
      ],
      [
        m.WorkEffortAssignmentRate,
        [
          m.WorkEffortAssignmentRate.WorkEffortPartyAssignment,
          m.WorkEffortAssignmentRate.RateType,
          m.WorkEffortAssignmentRate.Rate,
          m.WorkEffortAssignmentRate.Frequency,
        ],
      ],
      [
        m.WorkEffortFixedAssetAssignment,
        [
          m.WorkEffortFixedAssetAssignment.Assignment,
          m.WorkEffortFixedAssetAssignment.FixedAsset,
        ],
      ],
      [
        m.WorkEffortInventoryAssignment,
        [
          m.WorkEffortInventoryAssignment.InventoryItem,
          m.WorkEffortInventoryAssignment.Quantity,
        ],
      ],
      [
        m.WorkEffortInvoiceItemAssignment,
        [
          m.WorkEffortInvoiceItemAssignment.Assignment,
          m.WorkEffortInvoiceItemAssignment.WorkEffortInvoiceItem,
        ],
      ],
      [
        m.WorkEffortPartyAssignment,
        [
          m.WorkEffortPartyAssignment.Assignment,
          m.WorkEffortPartyAssignment.Party,
        ],
      ],
      [
        m.WorkEffortPurchaseOrderItemAssignment,
        [
          m.WorkEffortPurchaseOrderItemAssignment.Assignment,
          m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder,
          m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrderItem,
          m.WorkEffortPurchaseOrderItemAssignment.Quantity,
        ],
      ],
      [
        m.WorkRequirement,
        [
          m.WorkRequirement.RequirementNumber,
          m.WorkRequirement.OriginatorName,
          m.WorkRequirement.RequirementState,
        ],
      ],
      [
        m.WorkRequirementFulfillment,
        [
          m.WorkRequirementFulfillment.WorkEffortNumber,
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
