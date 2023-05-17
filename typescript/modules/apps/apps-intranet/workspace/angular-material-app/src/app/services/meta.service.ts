import {
  MetaService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  AssociationType,
  Composite,
  pluralize,
  PropertyType,
  RoleType,
} from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMetaService implements MetaService {
  singularNameByObject: Map<Composite | PropertyType, string>;
  pluralNameByObject: Map<Composite | PropertyType, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.singularNameByObject = new Map<Composite | PropertyType, string>([
      [m.Organisation, 'Company'],
      [m.Organisation.PurchaseOrdersWhereTakenViaSupplier, 'Purchase Order'],
      [m.Part.InventoryItemsWherePart, 'Inventory'],
      [m.Part.SupplierOfferingsWherePart, 'Supplier Offering'],
      [m.Party.PartyContactMechanismsWhereParty, 'ContactMechanism Usage'],
      [m.Party.PartyRelationshipsWhereParty, 'Party Relationship'],
      [m.Party.QuotesWhereReceiver, 'Quote'],
      [m.Party.RequestsWhereOriginator, 'Request for Quote'],
      [m.Party.SalesInvoicesWhereBillToCustomer, 'Sales Invoice'],
      [m.Party.SalesOrdersWhereBillToCustomer, 'Sales Order'],
      [m.PaymentApplication.PaymentWherePaymentApplication, 'Payment'],
      [
        m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem,
        'Purchase Invoice',
      ],
      [
        m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem,
        'Purchase Order',
      ],
      [m.QuoteItem.QuoteWhereQuoteItem, 'Quote'],
      [m.RequestItem.RequestWhereRequestItem, 'Request for Quote'],
      [
        m.SalesInvoice.AccountingTransactionsWhereInvoice,
        'Accounting Transaction',
      ],
      [m.SalesInvoice.RepeatingSalesInvoiceWhereSource, 'Repeating Invoice'],
      [m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, 'Sales Invoice'],
      [m.SalesOrderItem.SalesOrderWhereSalesOrderItem, 'Sales Order'],
      [
        m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem,
        'Inventory',
      ],
      [m.WorkEffort.ServiceEntriesWhereWorkEffort, 'Time Entry'],
    ]);

    this.pluralNameByObject = new Map<Composite | PropertyType, string>([
      [m.Organisation, 'Companies'],
      [m.Part.InventoryItemsWherePart, 'Inventory'],
      [m.Party.RequestsWhereOriginator, 'Requests for Quote'],
      [m.Product.PriceComponentsWhereProduct, 'Price Components'],
      [m.RequestItem.RequestWhereRequestItem, 'Requests for Quote'],
      [
        m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem,
        'Inventory',
      ],
      [m.SerialisedItem.WorkRequirementsWhereFixedAsset, 'Work Requirements'],
      [m.WorkEffort.ServiceEntriesWhereWorkEffort, 'Time Entries'],
      [m.WorkEffortInventoryAssignment.InventoryItem, 'Parts used'],
    ]);
  }

  singularName(metaObject: Composite | PropertyType): string {
    return this.singularNameByObject.get(metaObject) ?? metaObject.singularName;
  }

  pluralName(metaObject: Composite | PropertyType): string {
    return (
      this.pluralNameByObject.get(metaObject) ??
      pluralize(this.singularNameByObject.get(metaObject)) ??
      metaObject.pluralName
    );
  }
}
