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
import { Proposal, SalesOrder } from '@allors/default/workspace/domain';
import { PrintService } from '../../../actions/print/print.service';
import { CopyService } from '../../../actions/copy/copy.service';

@Component({
  selector: 'proposal-summary-panel',
  templateUrl: './proposal-summary-panel.component.html',
})
export class ProposalSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  proposal: Proposal;
  salesOrder: SalesOrder;
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
      p.Proposal({
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
      p.Quote({
        name: `${prefix}_salesOrder`,
        objectId: id,
        select: {
          SalesOrderWhereQuote: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.proposal = loaded.object<Proposal>(prefix);
    this.salesOrder = loaded.object<SalesOrder>(`${prefix}_salesOrder`);
  }

  public setReadyForProcessing(): void {
    this.invokeService
      .invoke(this.proposal.SetReadyForProcessing)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully set ready for processing.', 'close', {
          duration: 5000,
        });
      }, this.errorService.errorHandler);
  }

  public approve(): void {
    this.invokeService.invoke(this.proposal.Approve).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully approved.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  send() {
    this.invokeService.invoke(this.proposal.Send).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully sent.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  accept() {
    this.invokeService.invoke(this.proposal.Accept).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully accepted.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reopen(): void {
    this.invokeService.invoke(this.proposal.Reopen).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully reopened.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public revise(): void {
    this.invokeService.invoke(this.proposal.Revise).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully revised.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public cancel(): void {
    this.invokeService.invoke(this.proposal.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.invokeService.invoke(this.proposal.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public order(): void {
    this.invokeService.invoke(this.proposal.Order).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created a salesorder.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }
}
