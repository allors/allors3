import { tags } from '@allors/workspace/meta/default';

export interface MenuItem {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuItem[];
}

export const menu: MenuItem[] = [
  { title: 'Home', icon: 'home', link: '/' },
  {
    title: 'Contacts',
    icon: 'group',
    children: [{ tag: tags.Person }, { tag: tags.Organisation }, { tag: tags.CommunicationEvent }],
  },
  {
    title: 'Products',
    icon: 'label',
    children: [
      { tag: tags.Good, title: 'Goods' },
      { tag: tags.Part, title: 'Parts' },
      { tag: tags.Catalogue },
      { tag: tags.ProductCategory },
      { tag: tags.SerialisedItemCharacteristic, title: 'Characteristics' },
      { tag: tags.ProductType },
      { tag: tags.SerialisedItem, title: 'Serialised Assets' },
      { tag: tags.UnifiedGood, title: 'Unified Goods' },
    ],
  },
  {
    title: 'Sales',
    icon: 'credit_card',
    children: [{ tag: tags.RequestForQuote }, { tag: tags.ProductQuote }, { tag: tags.SalesOrder }, { tag: tags.SalesInvoice }],
  },
  {
    title: 'Purchasing',
    icon: 'local_shipping',
    children: [{ tag: tags.PurchaseOrder }, { tag: tags.PurchaseInvoice }],
  },
  {
    title: 'Shipments',
    icon: 'local_shipping',
    children: [{ tag: tags.Shipment }, { tag: tags.Carrier }],
  },
  {
    title: 'WorkEfforts',
    icon: 'schedule',
    children: [{ tag: tags.WorkEffort }],
  },
  {
    title: 'HR',
    icon: 'group',
    children: [{ tag: tags.PositionType }, { tag: tags.PositionTypeRate }],
  },
  {
    title: 'Accounting',
    icon: 'money',
    children: [{ tag: tags.ExchangeRate }],
  },
];
