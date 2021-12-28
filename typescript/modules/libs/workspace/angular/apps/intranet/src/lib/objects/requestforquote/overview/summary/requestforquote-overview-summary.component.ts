import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { M } from '@allors/workspace/meta/default';
import { RequestForQuote, Quote } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';

@Component({
  selector: 'requestforquote-overview-summary',
  templateUrl: './requestforquote-overview-summary.component.html',
  providers: [PanelService],
})
export class RequestForQuoteOverviewSummaryComponent {
  m: M;

  requestForQuote: RequestForQuote;
  quote: Quote;

  constructor(@Self() public panel: PanelService, public workspaceService: WorkspaceService, public refreshService: RefreshService, private saveService: SaveService, public snackBar: MatSnackBar, public navigation: NavigationService) {
    this.m = this.workspaceService.workspace.configuration.metaPopulation as M;

    panel.name = 'summary';

    const requestForQuotePullName = `${panel.name}_${this.m.RequestForQuote.tag}`;
    const productQuotePullName = `${panel.name}_${this.m.ProductQuote.tag}`;

    panel.onPull = (pulls) => {
      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      pulls.push(
        pull.RequestForQuote({
          name: requestForQuotePullName,
          objectId: this.panel.manager.id,
          include: {
            FullfillContactMechanism: {
              PostalAddress_Country: x,
            },
            RequestItems: {
              Product: x,
            },
            Originator: x,
            ContactPerson: x,
            RequestState: x,
            DerivedCurrency: x,
            CreatedBy: x,
            LastModifiedBy: x,
          },
        }),
        pull.RequestForQuote({
          name: productQuotePullName,
          objectId: this.panel.manager.id,
          select: {
            QuoteWhereRequest: x,
          },
        })
      );
    };

    panel.onPulled = (loaded) => {
      this.requestForQuote = loaded.object<RequestForQuote>(requestForQuotePullName);
      this.quote = loaded.object<Quote>(productQuotePullName);
    };
  }

  public cancel(): void {
    this.panel.manager.context.invoke(this.requestForQuote.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public reject(): void {
    this.panel.manager.context.invoke(this.requestForQuote.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public submit(): void {
    this.panel.manager.context.invoke(this.requestForQuote.Submit).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully submitted.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public hold(): void {
    this.panel.manager.context.invoke(this.requestForQuote.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully held.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }

  public createQuote(): void {
    this.panel.manager.context.invoke(this.requestForQuote.CreateQuote).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully created a quote.', 'close', { duration: 5000 });
    }, this.saveService.errorHandler);
  }
}
