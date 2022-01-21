import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/default/workspace/meta';
import {
  RequestForQuote,
  ProductQuote,
  SalesOrder,
} from '@allors/workspace/domain/default';
import {
  Action,
  NavigationService,
  PanelService,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { PrintService } from '../../../../actions/print/print.service';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  selector: 'productquote-overview-summary',
  templateUrl: './productquote-overview-summary.component.html',
  providers: [PanelService],
})
export class ProductQuoteOverviewSummaryComponent {
  m: M;

  productQuote: ProductQuote;
  salesOrder: SalesOrder;
  request: RequestForQuote;
  print: Action;

  constructor(
    @Self() public panel: PanelService,
    public workspaceService: WorkspaceService,
    public navigation: NavigationService,
    public printService: PrintService,
    private saveService: SaveService,
    public refreshService: RefreshService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.print = printService.print();

    panel.name = 'summary';

    const productQuotePullName = `${panel.name}_${this.m.ProductQuote.tag}`;
    const salesOrderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;
    const requestPullName = `${panel.name}_${this.m.RequestForQuote.tag}`;

    panel.onPull = (pulls) => {
      pulls.push(
        pull.ProductQuote({
          name: productQuotePullName,
          objectId: this.panel.manager.id,
          include: {
            QuoteItems: {
              Product: x,
              QuoteItemState: x,
            },
            Receiver: x,
            ContactPerson: x,
            QuoteState: x,
            CreatedBy: x,
            LastModifiedBy: x,
            Request: x,
            FullfillContactMechanism: {
              PostalAddress_Country: x,
            },
            PrintDocument: {
              Media: x,
            },
          },
        }),
        pull.ProductQuote({
          name: salesOrderPullName,
          objectId: this.panel.manager.id,
          select: {
            SalesOrderWhereQuote: x,
          },
        }),
        pull.ProductQuote({
          name: requestPullName,
          objectId: this.panel.manager.id,
          select: {
            Request: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.productQuote = loaded.object<ProductQuote>(productQuotePullName);
      this.salesOrder = loaded.object<SalesOrder>(salesOrderPullName);
      this.request = loaded.object<RequestForQuote>(requestPullName);
    };
  }

  public setReadyForProcessing(): void {
    this.panel.manager.context
      .invoke(this.productQuote.SetReadyForProcessing)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully set ready for processing.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public approve(): void {
    this.panel.manager.context
      .invoke(this.productQuote.Approve)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully approved.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  send() {
    this.panel.manager.context.invoke(this.productQuote.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully sent.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  accept() {
    this.panel.manager.context
      .invoke(this.productQuote.Accept)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully accepted.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.context
      .invoke(this.productQuote.Reopen)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully reopened.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.context
      .invoke(this.productQuote.Revise)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully revised.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public order(): void {
    this.panel.manager.context.invoke(this.productQuote.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created a salesorder.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.context
      .invoke(this.productQuote.Cancel)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully cancelled.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public reject(): void {
    this.panel.manager.context
      .invoke(this.productQuote.Reject)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully rejected.', 'close', {
          duration: 5000,
        });
      }, this.saveService.errorHandler);
  }

  public Order(): void {
    this.panel.manager.context.invoke(this.productQuote.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('SalesOrder successfully created.', 'close', {
        duration: 5000,
      });
    }, this.saveService.errorHandler);
  }
}
