import { NavigationService } from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AppNavigationService extends NavigationService {
  listByComposite: Map<Composite, string>;
  overviewByComposite: Map<Composite, any>;

  constructor(private router: Router, workspaceService: WorkspaceService) {
    super();

    this.listByComposite = new Map();
    this.overviewByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    const define = (composite: Composite, list: string, overview?: string) => {
      this.listByComposite.set(composite, list);
      this.overviewByComposite.set(composite, overview);
    };

    // Navigation
    define(m.Person, '/contacts/people', '/contacts/person/:id');
    define(
      m.Organisation,
      '/contacts/organisations',
      '/contacts/organisation/:id'
    );
    define(m.CommunicationEvent, '/contacts/communicationevents');

    define(
      m.RequestForQuote,
      '/sales/requestsforquote',
      '/sales/requestforquote/:id'
    );
    define(m.ProductQuote, '/sales/productquotes', '/sales/productquote/:id');
    define(m.SalesOrder, '/sales/salesorders', '/sales/salesorder/:id');
    define(m.SalesInvoice, '/sales/salesinvoices', '/sales/salesinvoice/:id');

    define(m.Good, '/products/goods');
    define(m.NonUnifiedGood, '/products/goods', '/products/nonunifiedgood/:id');
    define(m.Part, '/products/parts');
    define(m.NonUnifiedPart, '/products/parts', '/products/nonunifiedpart/:id');
    define(m.Catalogue, '/products/catalogues');
    define(m.ProductCategory, '/products/productcategories');
    define(
      m.SerialisedItemCharacteristic,
      '/products/serialiseditemcharacteristics'
    );
    define(m.ProductType, '/products/producttypes');
    define(
      m.SerialisedItem,
      '/products/serialiseditems',
      '/products/serialisedItem/:id'
    );
    define(
      m.UnifiedGood,
      '/products/unifiedgoods',
      '/products/unifiedgood/:id'
    );

    define(
      m.PurchaseOrder,
      '/purchasing/purchaseorders',
      '/purchasing/purchaseorder/:id'
    );
    define(
      m.PurchaseInvoice,
      '/purchasing/purchaseinvoices',
      '/purchasing/purchaseinvoice/:id'
    );

    define(m.Shipment, '/shipment/shipments');
    define(
      m.CustomerShipment,
      '/shipment/shipments',
      '/shipment/customershipment/:id'
    );
    define(
      m.PurchaseShipment,
      '/shipment/shipments',
      '/shipment/purchaseshipment/:id'
    );
    define(m.Carrier, '/shipment/carriers');

    define(m.WorkEffort, '/workefforts/workefforts');
    define(
      m.WorkRequirement,
      '/workefforts/workrequirements',
      '/workefforts/workrequirement/:id'
    );
    define(m.WorkTask, '/workefforts/workefforts', '/workefforts/worktask/:id');

    define(m.PositionType, '/humanresource/positiontypes');
    define(m.PositionTypeRate, '/humanresource/positiontyperates');

    define(m.TaskAssignment, '/workflow/taskassignments');

    define(m.ExchangeRate, '/accounting/exchangerates');
  }

  hasList(objectType: Composite): boolean {
    return this.listByComposite.has(objectType);
  }

  listUrl(objectType: Composite) {
    return this.listByComposite.get(objectType);
  }

  list(objectType: Composite) {
    const url = this.listUrl(objectType);
    if (url != null) {
      this.router.navigate([url]);
    }
  }

  hasOverview(obj: IObject): boolean {
    return this.overviewByComposite.has(obj.strategy.cls);
  }

  overviewUrl(objectType: Composite) {
    return this.overviewByComposite.get(objectType);
  }

  overview(object: IObject) {
    if (object) {
      const url = this.overviewUrl(object.strategy.cls)?.replace(
        `:id`,
        object.strategy.id
      );
      if (url != null) {
        this.router.navigate([url]);
      }
    }
  }
}
