import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  Action,
  ErrorService,
  InvokeService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ProductQuote,
  RequestForQuote,
  SalesOrder,
} from '@allors/default/workspace/domain';
import { PrintService } from '../../../actions/print/print.service';
import { CopyService } from '../../../actions/copy/copy.service';

@Component({
  selector: 'productquote-summary-panel',
  templateUrl: './productquote-summary-panel.component.html',
})
export class ProductQuoteSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  productQuote: ProductQuote;
  salesOrder: SalesOrder;
  request: RequestForQuote;
  print: Action;
  copy: Action;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    printService: PrintService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService,
    public navigation: NavigationService,
    public copyService: CopyService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    this.print = printService.print();
    this.copy = copyService.copy();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.ProductQuote({
        name: prefix,
        objectId: id,
        include: {
          QuoteItems: {
            Product: {},
            QuoteItemState: {},
          },
          Receiver: {},
          ContactPerson: {},
          QuoteState: {},
          CreatedBy: {},
          LastModifiedBy: {},
          Request: {},
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
          PrintDocument: {
            Media: {},
          },
        },
      }),
      p.ProductQuote({
        name: `${prefix}_salesOrder`,
        objectId: id,
        select: {
          SalesOrderWhereQuote: {},
        },
      }),
      p.ProductQuote({
        name: `${prefix}_request`,
        objectId: id,
        select: {
          Request: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.productQuote = loaded.object<ProductQuote>(prefix);
    this.salesOrder = loaded.object<SalesOrder>(`${prefix}_salesOrder`);
    this.request = loaded.object<RequestForQuote>(`${prefix}_request`);
  }

  public setReadyForProcessing(): void {
    this.invokeService
      .invoke(this.productQuote.SetReadyForProcessing)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully set ready for processing.', 'close', {
          duration: 5000,
        });
      }, this.errorService.errorHandler);
  }

  public approve(): void {
    this.invokeService.invoke(this.productQuote.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  send() {
    this.invokeService.invoke(this.productQuote.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully sent.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  accept() {
    this.invokeService.invoke(this.productQuote.Accept).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully accepted.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.invokeService.invoke(this.productQuote.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.invokeService.invoke(this.productQuote.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public order(): void {
    this.invokeService.invoke(this.productQuote.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created a salesorder.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.invokeService.invoke(this.productQuote.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.invokeService.invoke(this.productQuote.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }
}
