import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { RequestForQuote, ProductQuote, SalesOrder } from '@allors/workspace/domain/default';
import { Action, NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { PrintService } from '../../../../actions/print/print.service';

@Component({
  // tslint:disable-next-line:component-selector
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

    public navigation: NavigationService,
    public printService: PrintService,
    private saveService: SaveService,
    public refreshService: RefreshService,
    public snackBar: MatSnackBar
  ) {
    this.m = this.allors.workspace.configuration.metaPopulation as M;

    this.print = printService.print();

    panel.name = 'summary';

    const productQuotePullName = `${panel.name}_${this.m.ProductQuote.tag}`;
    const salesOrderPullName = `${panel.name}_${this.m.SalesOrder.tag}`;
    const requestPullName = `${panel.name}_${this.m.RequestForQuote.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

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
      this.productQuote = loaded.objects[productQuotePullName] as ProductQuote;
      this.salesOrder = loaded.objects[salesOrderPullName] as SalesOrder;
      this.request = loaded.objects[requestPullName] as RequestForQuote;
    };
  }

  public setReadyForProcessing(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.SetReadyForProcessing).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully set ready for processing.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public approve(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  send() {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully sent.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  accept() {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Accept).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully accepted.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reopen(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public revise(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public order(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created a salesorder.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public cancel(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reject(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public Order(): void {
    this.panel.manager.client.invokeReactive(this.panel.manager.session, this.productQuote.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('SalesOrder successfully created.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }
}
