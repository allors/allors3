import { FilterDefinition } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Sorter } from '@allors/workspace/angular/base';
import { Composite } from '@allors/workspace/meta/system';

function nav(composite: Composite, list: string, overview?: string) {
  composite._.list = list;
  composite._.overview = overview;
}

export function configure(m: M) {
  // Menu
  m._.menu = [
    { title: 'Home', icon: 'home', link: '/' },
    {
      title: 'Contacts',
      icon: 'group',
      children: [{ objectType: m.Person }, { objectType: m.Organisation }, { objectType: m.CommunicationEvent }],
    },
    {
      title: 'Products',
      icon: 'label',
      children: [
        { objectType: m.Good, title: 'Goods' },
        { objectType: m.Part, title: 'Parts' },
        { objectType: m.Catalogue },
        { objectType: m.ProductCategory },
        { objectType: m.SerialisedItemCharacteristic, title: 'Characteristics' },
        { objectType: m.ProductType },
        { objectType: m.SerialisedItem, title: 'Serialised Assets' },
        { objectType: m.UnifiedGood, title: 'Unified Goods' },
      ],
    },
    {
      title: 'Sales',
      icon: 'credit_card',
      children: [{ objectType: m.RequestForQuote }, { objectType: m.ProductQuote }, { objectType: m.SalesOrder }, { objectType: m.SalesInvoice }],
    },
    {
      title: 'Purchasing',
      icon: 'local_shipping',
      children: [{ objectType: m.PurchaseOrder }, { objectType: m.PurchaseInvoice }],
    },
    {
      title: 'Shipments',
      icon: 'local_shipping',
      children: [{ objectType: m.Shipment }, { objectType: m.Carrier }],
    },
    {
      title: 'WorkEfforts',
      icon: 'schedule',
      children: [{ objectType: m.WorkEffort }],
    },
    {
      title: 'HR',
      icon: 'group',
      children: [{ objectType: m.PositionType }, { objectType: m.PositionTypeRate }],
    },
    {
      title: 'Accounting',
      icon: 'money',
      children: [{ objectType: m.ExchangeRate }],
    },
  ];

  // Navigation
  nav(m.Person, '/contacts/people', '/contacts/person/:id');
  nav(m.Organisation, '/contacts/organisations', '/contacts/organisation/:id');
  nav(m.CommunicationEvent, '/contacts/communicationevents');

  nav(m.RequestForQuote, '/sales/requestsforquote', '/sales/requestforquote/:id');
  nav(m.ProductQuote, '/sales/productquotes', '/sales/productquotes/:id');
  nav(m.SalesOrder, '/sales/salesorders', '/sales/salesorder/:id');
  nav(m.SalesInvoice, '/sales/salesinvoices', '/sales/salesinvoice/:id');

  nav(m.Good, '/products/goods');
  nav(m.NonUnifiedGood, '/products/goods', '/products/nonunifiedgood/:id');
  nav(m.Part, '/products/parts');
  nav(m.NonUnifiedPart, '/products/parts', '/products/nonunifiedpart/:id');
  nav(m.Catalogue, '/products/catalogues');
  nav(m.ProductCategory, '/products/productcategories');
  nav(m.SerialisedItemCharacteristic, '/products/serialiseditemcharacteristics');
  nav(m.ProductType, '/products/producttypes');
  nav(m.SerialisedItem, '/products/serialiseditems', '/products/serialisedItem/:id');
  nav(m.UnifiedGood, '/products/unifiedgoods', '/products/unifiedgood/:id');

  nav(m.PurchaseOrder, '/purchasing/purchaseorders', '/purchasing/purchasingpurchaseorder/:id');
  nav(m.PurchaseInvoice, '/purchasing/purchaseinvoices', '/purchasing/purchasingpurchaseinvoice/:id');

  nav(m.Shipment, '/shipment/shipments');
  nav(m.CustomerShipment, '/shipment/shipments', '/shipment/customershipment/:id');
  nav(m.PurchaseShipment, '/shipment/shipments', '/shipment/purchaseshipment/:id');
  nav(m.Carrier, '/shipment/carriers');

  nav(m.WorkEffort, '/workefforts/workefforts');
  nav(m.WorkTask, '/workefforts/workefforts', '/workefforts/worktask/:id');

  nav(m.PositionType, '/humanresource/positiontypes');
  nav(m.PositionTypeRate, '/humanresource/positiontyperates');

  nav(m.TaskAssignment, '/workflow/taskassignments');

  nav(m.ExchangeRate, '/accounting/exchangerates');

  // Filter & Sort
  m.Person._.filterDefinition = new FilterDefinition({
    kind: 'And',
    operands: [
      {
        kind: 'Like',
        roleType: m.Person.FirstName,
        parameter: 'firstName',
      },
      {
        kind: 'Like',
        roleType: m.Person.LastName,
        parameter: 'lastName',
      },
      {
        kind: 'Like',
        roleType: m.Person.UserEmail,
        parameter: 'email',
      },
    ],
  });
  m.Person._.sorter = new Sorter({
    firstName: m.Person.FirstName,
    lastName: m.Person.LastName,
    email: m.Person.UserEmail,
  });

  m.Organisation._.filterDefinition = new FilterDefinition({
    kind: 'And',
    operands: [
      {
        kind: 'Like',
        roleType: m.Organisation.Name,
        parameter: 'name',
      },
    ],
  });
  m.Organisation._.sorter = new Sorter({ name: m.Organisation.Name });
}
