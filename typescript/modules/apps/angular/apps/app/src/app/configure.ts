import { FilterDefinition, IAngularComposite, IAngularMetaService } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';
import { Sorter } from '@allors/workspace/angular/base';
import { Composite } from '@allors/workspace/meta/system';

function nav(composite: IAngularComposite, list: string, overview?: string) {
  composite.list = list;
  composite.overview = overview;
}

export function configure(m: M, angularMeta: IAngularMetaService) {
  const a = (...composites: Composite[]) => composites.map((v) => angularMeta.for(v));

  const [person, organisation, communicationEvent] = a(m.Person, m.Organisation, m.CommunicationEvent);
  const [requestForQuote, productQuote, salesorder, salesinvoice] = a(m.RequestForQuote, m.ProductQuote, m.SalesOrder, m.SalesInvoice);
  const [good, nonUnifiedGood, part, nonUnifiedPart, catalogue, productCategory, serialisedItemCharacteristic, productType, serialisedItem, unifiedGood] = a(
    m.Good,
    m.NonUnifiedGood,
    m.Part,
    m.NonUnifiedPart,
    m.Catalogue,
    m.ProductCategory,
    m.SerialisedItemCharacteristic,
    m.ProductType,
    m.SerialisedItem,
    m.UnifiedGood
  );
  const [purchaseOrder, purchaseInvoice] = a(m.PurchaseOrder, m.PurchaseInvoice);
  const [shipment, customerShipment, purchaseShipment, carrier] = a(m.Shipment, m.CustomerShipment, m.PurchaseShipment, m.Carrier);
  const [workEffort] = a(m.WorkEffort);
  const [positionType, positionTypeRate] = a(m.PositionType, m.PositionTypeRate);
  const [taskAssignment] = a(m.TaskAssignment);
  const [exchangeRate] = a(m.ExchangeRate);

  // Navigation
  nav(person, '/contacts/people', '/contacts/person/:id');
  nav(organisation, '/contacts/organisations', '/contacts/organisation/:id');
  nav(communicationEvent, '/contacts/communicationevents');

  nav(requestForQuote, '/sales/requestsforquote', '/sales/requestforquote/:id');
  nav(productQuote, '/sales/productquotes', '/sales/productquotes/:id');

  // Filter & Sort
  person.filterDefinition = new FilterDefinition({
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
  person.sorter = new Sorter({
    firstName: m.Person.FirstName,
    lastName: m.Person.LastName,
    email: m.Person.UserEmail,
  });

  organisation.filterDefinition = new FilterDefinition({
    kind: 'And',
    operands: [
      {
        kind: 'Like',
        roleType: m.Organisation.Name,
        parameter: 'name',
      },
    ],
  });
  organisation.sorter = new Sorter({ name: m.Organisation.Name });
}
