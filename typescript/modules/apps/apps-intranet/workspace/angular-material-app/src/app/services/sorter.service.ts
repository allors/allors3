import {
  Sorter,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppSorterService implements SorterService {
  sorterByComposite: Map<Composite, Sorter>;

  constructor(workspaceService: WorkspaceService) {
    this.sorterByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    const define = (composite: Composite, sorter: Sorter) => {
      this.sorterByComposite.set(composite, sorter);
    };

    define(
      m.Carrier,
      new Sorter({
        name: m.Carrier.Name,
      })
    );

    define(
      m.Catalogue,
      new Sorter({
        name: m.Catalogue.Name,
        description: m.Catalogue.Description,
        scope: m.Scope.Name,
      })
    );

    define(
      m.CommunicationEvent,
      new Sorter({
        subject: m.CommunicationEvent.Subject,
        lastModifiedDate: m.CommunicationEvent.LastModifiedDate,
      })
    );

    define(
      m.EmailMessage,
      new Sorter({
        created: m.EmailMessage.DateCreated,
        sent: m.EmailMessage.DateSent,
        subject: m.EmailMessage.Subject,
      })
    );

    define(
      m.ExchangeRate,
      new Sorter({
        validFrom: m.ExchangeRate.ValidFrom,
        from: m.ExchangeRate.FromCurrency,
        to: m.ExchangeRate.ToCurrency,
      })
    );

    define(
      m.Facility,
      new Sorter({
        name: m.Facility.Name,
        type: m.Facility.FacilityTypeName,
        owner: m.Facility.OwnerName,
        parent: m.Facility.ParentName,
      })
    );

    define(
      m.GeneralLedgerAccount,
      new Sorter({
        referenceNumber: m.GeneralLedgerAccount.ReferenceNumber,
        name: m.GeneralLedgerAccount.Name,
      })
    );

    define(
      m.Good,
      new Sorter({
        name: [m.Good.Name],
      })
    );

    define(
      m.NonUnifiedPart,
      new Sorter({
        name: m.NonUnifiedPart.Name,
        partNo: m.NonUnifiedPart.ProductNumber,
        type: m.NonUnifiedPart.ProductTypeName,
        categories: m.NonUnifiedPart.PartCategoriesDisplayName,
        brand: m.NonUnifiedPart.BrandName,
        model: m.NonUnifiedPart.ModelName,
        kind: m.NonUnifiedPart.InventoryItemKindName,
        lastModifiedDate: m.UnifiedProduct.LastModifiedDate,
      })
    );

    define(
      m.Organisation,
      new Sorter({
        name: m.Organisation.Name,
        lastModifiedDate: m.Organisation.LastModifiedDate,
      })
    );

    define(
      m.Part,
      new Sorter({
        name: m.Part.Name,
        partNo: m.Part.ProductNumber,
        type: m.Part.ProductTypeName,
        categories: m.Part.PartCategoriesDisplayName,
        brand: m.Part.BrandName,
        model: m.Part.ModelName,
        kind: m.Part.InventoryItemKindName,
        lastModifiedDate: m.Part.LastModifiedDate,
      })
    );

    define(
      m.Person,
      new Sorter({
        name: [m.Person.FirstName, m.Person.LastName],
        lastModifiedDate: m.Person.LastModifiedDate,
      })
    );

    define(
      m.PositionType,
      new Sorter({
        title: m.PositionType.Title,
      })
    );

    define(
      m.PositionTypeRate,
      new Sorter({
        rate: m.PositionTypeRate.Rate,
        from: m.PositionTypeRate.FromDate,
        through: m.PositionTypeRate.ThroughDate,
      })
    );

    define(
      m.ProductCategory,
      new Sorter({
        name: m.Catalogue.Name,
        description: m.Catalogue.Description,
        scope: m.Scope.Name,
      })
    );

    define(
      m.ProductQuote,
      new Sorter({
        number: m.Quote.SortableQuoteNumber,
        description: m.Quote.Description,
        validThroughDate: m.Quote.ValidThroughDate,
        lastModifiedDate: m.Quote.LastModifiedDate,
      })
    );

    define(
      m.ProductType,
      new Sorter({
        name: m.ProductType.Name,
      })
    );

    define(
      m.Proposal,
      new Sorter({
        number: m.Quote.SortableQuoteNumber,
        description: m.Quote.Description,
        validThroughDate: m.Quote.ValidThroughDate,
        lastModifiedDate: m.Quote.LastModifiedDate,
      })
    );

    define(
      m.PurchaseInvoice,
      new Sorter({
        number: m.PurchaseInvoice.SortableInvoiceNumber,
        type: m.PurchaseInvoice.PurchaseInvoiceType,
        reference: m.PurchaseInvoice.CustomerReference,
        dueDate: m.PurchaseInvoice.DueDate,
        totalExVat: m.PurchaseInvoice.TotalExVat,
        totalIncVat: m.PurchaseInvoice.TotalIncVat,
        lastModifiedDate: m.PurchaseInvoice.LastModifiedDate,
      })
    );

    define(
      m.PurchaseOrder,
      new Sorter({
        number: m.PurchaseOrder.SortableOrderNumber,
        customerReference: m.PurchaseOrder.CustomerReference,
        totalExVat: m.PurchaseOrder.TotalExVat,
        totalIncVat: m.PurchaseOrder.TotalIncVat,
        lastModifiedDate: m.PurchaseOrder.LastModifiedDate,
      })
    );

    define(
      m.RequestForQuote,
      new Sorter({
        number: m.Request.SortableRequestNumber,
        description: m.Request.Description,
        responseRequired: m.Request.RequiredResponseDate,
        lastModifiedDate: m.Request.LastModifiedDate,
      })
    );

    define(
      m.SalesInvoice,
      new Sorter({
        number: m.SalesInvoice.SortableInvoiceNumber,
        type: m.SalesInvoice.SalesInvoiceType,
        invoiceDate: m.SalesInvoice.InvoiceDate,
        description: m.SalesInvoice.Description,
        lastModifiedDate: m.SalesInvoice.LastModifiedDate,
      })
    );

    define(
      m.SalesOrder,
      new Sorter({
        number: m.SalesOrder.SortableOrderNumber,
        customerReference: m.SalesOrder.CustomerReference,
        lastModifiedDate: m.SalesOrder.LastModifiedDate,
      })
    );

    define(
      m.SerialisedItem,
      new Sorter({
        id: [m.SerialisedItem.ItemNumber],
        categories: [m.SerialisedItem.ProductCategoriesDisplayName],
        name: [m.SerialisedItem.DisplayName],
        availability: [m.SerialisedItem.SerialisedItemAvailabilityName],
      })
    );

    define(
      m.SerialisedItemCharacteristic,
      new Sorter({
        name: m.SerialisedItemCharacteristicType.Name,
        uom: m.UnitOfMeasure.Name,
        active: m.SerialisedItemCharacteristicType.IsActive,
      })
    );

    define(
      m.Shipment,
      new Sorter({
        number: m.Shipment.SortableShipmentNumber,
        from: m.Shipment.ShipFromParty,
        to: m.Shipment.ShipToParty,
        lastModifiedDate: m.Shipment.LastModifiedDate,
      })
    );

    define(
      m.UnifiedGood,
      new Sorter({
        name: [m.UnifiedGood.Name],
        id: [m.UnifiedGood.ProductNumber],
        lastModifiedDate: m.UnifiedGood.LastModifiedDate,
      })
    );

    define(
      m.WorkEffort,
      new Sorter({
        number: [m.WorkEffort.SortableWorkEffortNumber],
        name: [m.WorkEffort.Name],
        description: [m.WorkEffort.Description],
        lastModifiedDate: m.Person.LastModifiedDate,
      })
    );

    define(
      m.WorkRequirement,
      new Sorter({
        number: [m.WorkRequirement.SortableRequirementNumber],
        state: [m.WorkRequirement.RequirementStateName],
        priority: [m.WorkRequirement.SortableRequirementNumber],
        equipment: [m.WorkRequirement.FixedAssetName],
        location: [m.WorkRequirement.Location],
        lastModifiedDate: m.WorkRequirement.LastModifiedDate,
      })
    );
  }

  sorter(composite: Composite): Sorter {
    return this.sorterByComposite.get(composite);
  }
}
