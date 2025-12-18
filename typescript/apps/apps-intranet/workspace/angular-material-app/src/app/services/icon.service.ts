import { M } from '@allors/default/workspace/meta';
import { Composite, RelationType } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

import { Injectable } from '@angular/core';
import { IconService } from '@allors/base/workspace/angular-material/application';

@Injectable()
export class AppIconService implements IconService {
  iconByComposite: Map<Composite | RelationType, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.iconByComposite = new Map();
    this.iconByComposite.set(m.Country, 'public');
    this.iconByComposite.set(m.Organisation, 'domain');
    this.iconByComposite.set(m.CustomerShipment, 'local_shipping');
    this.iconByComposite.set(m.NonUnifiedPart, 'business');
    this.iconByComposite.set(m.Organisation, 'business');
    this.iconByComposite.set(m.Person, 'person');
    this.iconByComposite.set(m.ProductQuote, 'business');
    this.iconByComposite.set(m.PurchaseInvoice, 'business');
    this.iconByComposite.set(m.PurchaseOrder, 'business');
    this.iconByComposite.set(m.PurchaseShipment, 'local_shipping');
    this.iconByComposite.set(m.RequestForQuote, 'business');
    this.iconByComposite.set(m.SalesInvoice, 'business');
    this.iconByComposite.set(m.SalesOrder, 'business');
    this.iconByComposite.set(m.SerialisedItem, 'business');
    this.iconByComposite.set(m.UnifiedGood, 'business');
    this.iconByComposite.set(m.WorkRequirement, 'business');
    this.iconByComposite.set(m.WorkTask, 'business');
  }

  icon(meta: Composite | RelationType): string {
    return this.iconByComposite.get(meta);
  }
}
