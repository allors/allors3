import { Component, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  Action,
  ErrorService,
  InvokeService,
  MediaService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  Person,
  ProductQuote,
  PurchaseInvoice,
  PurchaseOrder,
  Quote,
  RequestForQuote,
  SalesOrder,
  User,
} from '@allors/default/workspace/domain';

@Component({
  selector: 'requestforquote-overview-summary',
  templateUrl: './requestforquote-overview-summary.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class RequestForQuoteOverviewSummaryComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  requestForQuote: RequestForQuote;
  quote: Quote;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.RequestForQuote({
        name: prefix,
        objectId: id,
        include: {
          FullfillContactMechanism: {
            PostalAddress_Country: {},
          },
          RequestItems: {
            Product: {},
          },
          Originator: {},
          ContactPerson: {},
          RequestState: {},
          DerivedCurrency: {},
          CreatedBy: {},
          LastModifiedBy: {},
        },
      }),
      p.RequestForQuote({
        name: `${prefix}_productQuote`,
        objectId: id,
        select: {
          QuoteWhereRequest: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.requestForQuote = loaded.object<RequestForQuote>(prefix);
    this.quote = loaded.object<Quote>(`${prefix}_productQuote`);
  }

  public cancel(): void {
    this.invokeService.invoke(this.requestForQuote.Cancel).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully cancelled.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public reject(): void {
    this.invokeService.invoke(this.requestForQuote.Reject).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully rejected.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public submit(): void {
    this.invokeService.invoke(this.requestForQuote.Submit).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully submitted.', 'close', {
        duration: 5000,
      });
    }, this.errorService.errorHandler);
  }

  public hold(): void {
    this.invokeService.invoke(this.requestForQuote.Hold).subscribe(() => {
      this.refreshService.refresh();
      this.snackBar.open('Successfully held.', 'close', { duration: 5000 });
    }, this.errorService.errorHandler);
  }

  public createQuote(): void {
    this.invokeService
      .invoke(this.requestForQuote.CreateQuote)
      .subscribe(() => {
        this.refreshService.refresh();
        this.snackBar.open('Successfully created a quote.', 'close', {
          duration: 5000,
        });
      }, this.errorService.errorHandler);
  }
}
