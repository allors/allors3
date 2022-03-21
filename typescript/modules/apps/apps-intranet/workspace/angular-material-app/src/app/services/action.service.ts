import { Injectable } from '@angular/core';
import {
  Action,
  ActionService,
  ActionTarget,
  CreateService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { MethodActionService } from '@allors/base/workspace/angular-material/application';
import { PrintService } from '@allors/apps-intranet/workspace/angular-material';

@Injectable()
export class AppActionService implements ActionService {
  actionByObjectType: Map<Composite, Action[]>;

  constructor(
    workspaceService: WorkspaceService,
    methodActionService: MethodActionService,
    createService: CreateService,
    printService: PrintService
  ) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.actionByObjectType = new Map<Composite, Action[]>([
      [
        m.NonSerialisedInventoryItem,
        [
          {
            name: 'changeinventory',
            displayName: () => 'Change Inventory',
            description: () => '',
            disabled: () => false,
            execute: (target: ActionTarget) => {
              if (!Array.isArray(target)) {
                createService.create({
                  kind: 'CreateRequest',
                  objectType: m.InventoryItemTransaction,
                  initializer: {
                    id: target.id,
                    propertyType: m.InventoryItemTransaction.InventoryItem,
                  },
                });
              }
            },
            result: null,
          },
        ],
      ],
      // TODO: KOEN new pull printdocument
      // [m.PurchaseOrder, [printService.print(m.PurchaseOrder.PrintDocument)]],
      [
        m.PurchaseOrderItem,
        [
          methodActionService.create(m.PurchaseOrderItem.Cancel),
          methodActionService.create(m.PurchaseOrderItem.Reject),
          methodActionService.create(m.PurchaseOrderItem.Reopen),
          methodActionService.create(m.PurchaseOrderItem.QuickReceive),
          methodActionService.create(m.PurchaseOrderItem.Return),
        ],
      ],
      [
        m.QuoteItem,
        [
          methodActionService.create(m.QuoteItem.Cancel),
          methodActionService.create(m.QuoteItem.Reject),
          methodActionService.create(m.QuoteItem.Submit),
        ],
      ],
      [
        m.RequestItem,
        [
          methodActionService.create(m.RequestItem.Cancel),
          methodActionService.create(m.RequestItem.Hold),
          methodActionService.create(m.RequestItem.Submit),
        ],
      ],
      [
        m.SalesOrderItem,
        [
          methodActionService.create(m.SalesOrderItem.Cancel),
          methodActionService.create(m.SalesOrderItem.Reject),
          methodActionService.create(m.SalesOrderItem.Reopen),
        ],
      ],
      [
        m.SerialisedInventoryItem,
        [
          {
            name: 'changeinventory',
            displayName: () => 'Change Inventory',
            description: () => '',
            disabled: () => false,
            execute: (target: ActionTarget) => {
              if (!Array.isArray(target)) {
                createService.create({
                  kind: 'CreateRequest',
                  objectType: m.InventoryItemTransaction,
                  initializer: {
                    id: target.id,
                    propertyType: m.InventoryItemTransaction.InventoryItem,
                  },
                });
              }
            },
            result: null,
          },
        ],
      ],
      [
        m.SerialisedItem,
        [
          {
            name: 'changeinventory',
            displayName: () => 'Change Inventory',
            description: () => '',
            disabled: () => false,
            execute: (target: ActionTarget) => {
              if (!Array.isArray(target)) {
                createService.create({
                  kind: 'CreateRequest',
                  objectType: m.InventoryItemTransaction,
                  initializer: {
                    id: target.id,
                    propertyType: m.InventoryItemTransaction.SerialisedItem,
                  },
                });
              }
            },
            result: null,
          },
        ],
      ],
    ]);
  }

  action(objectType: Composite): Action[] {
    return this.actionByObjectType.get(objectType) ?? [];
  }
}
