import {
  MenuItem,
  MenuService,
} from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMenuService implements MenuService {
  private _menu: MenuItem[];

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    this._menu = [
      { title: 'Home', icon: 'home', link: '/' },
      {
        title: 'Contacts',
        icon: 'group',
        children: [
          { objectType: m.Person },
          { objectType: m.Organisation },
          { objectType: m.CommunicationEvent },
        ],
      },
      {
        title: 'Products',
        icon: 'label',
        children: [
          { objectType: m.Good, title: 'Goods' },
          { objectType: m.Part, title: 'Parts' },
          { objectType: m.Catalogue },
          { objectType: m.ProductCategory },
          {
            objectType: m.SerialisedItemCharacteristic,
            title: 'Characteristics',
          },
          { objectType: m.SerialisedItem, title: 'Serialised Assets' },
          { objectType: m.UnifiedGood, title: 'Unified Goods' },
          { objectType: m.Brand },
          { objectType: m.Model },
          { objectType: m.ProductType },
          { objectType: m.Facility },
        ],
      },
      {
        title: 'Sales',
        icon: 'credit_card',
        children: [
          { objectType: m.RequestForQuote },
          { objectType: m.ProductQuote },
          { objectType: m.Proposal },
          { objectType: m.SalesOrder },
          { objectType: m.SalesInvoice },
        ],
      },
      {
        title: 'Purchasing',
        icon: 'local_shipping',
        children: [
          { objectType: m.PurchaseOrder },
          { objectType: m.PurchaseInvoice },
        ],
      },
      {
        title: 'Shipments',
        icon: 'local_shipping',
        children: [{ objectType: m.Shipment }, { objectType: m.Carrier }],
      },
      {
        title: 'WorkEfforts',
        icon: 'schedule',
        children: [
          { objectType: m.WorkRequirement },
          { objectType: m.WorkEffort },
        ],
      },
      {
        title: 'HR',
        icon: 'group',
        children: [
          { objectType: m.PositionType },
          { objectType: m.PositionTypeRate },
        ],
      },
      {
        title: 'Accounting',
        icon: 'money',
        children: [
          { objectType: m.ExchangeRate },
          { objectType: m.GeneralLedgerAccount, title: 'GL Accounts' },
          { objectType: m.AccountingTransaction, title: 'Transactions' },
        ],
      },
      {
        title: 'Admin',
        icon: 'admin_panel_settings',
        children: [
          { objectType: m.EmailMessage },
          { objectType: m.InvoiceItemType },
          { objectType: m.VatRegime, title: 'VAT Regimes' },
          { objectType: m.IrpfRegime, title: 'IRPF Regimes' },
          { objectType: m.VatClause, title: 'VAT Clauses' },
        ],
      },
    ];
  }

  menu(): MenuItem[] {
    return this._menu;
  }
}
