import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  GeneralLedgerAccount,
  InternalOrganisationInvoiceItemTypeSettings,
  InvoiceItemType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './invoiceitemtype-form.component.html',
  providers: [ContextService],
})
export class InvoiceItemTypeFormComponent extends AllorsFormComponent<InvoiceItemType> {
  public m: M;

  salesAccount: GeneralLedgerAccount;
  purchaseAccount: GeneralLedgerAccount;
  glAccounts: GeneralLedgerAccount[];
  invoiceItemTypeSettings: InternalOrganisationInvoiceItemTypeSettings;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.OrganisationGlAccount({
        select: {
          GeneralLedgerAccount: {},
        },
        predicate: {
          kind: 'Equals',
          propertyType: m.OrganisationGlAccount.InternalOrganisation,
          value: this.internalOrganisationId.value,
        },
      }),
      p.InternalOrganisation({
        name: 'SettingsForInvoiceItemTypes',
        objectId: this.internalOrganisationId.value,
        select: {
          SettingsForAccounting: {
            SettingsForInvoiceItemTypes: {
              include: {
                InvoiceItemType: {},
                SalesGeneralLedgerAccount: {},
                PurchaseGeneralLedgerAccount: {},
              },
            },
          },
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.InvoiceItemType({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.glAccounts = pullResult.collection<GeneralLedgerAccount>(
      this.m.GeneralLedgerAccount
    );

    const invoiceItemTypeSettingses = pullResult.collection(
      'SettingsForInvoiceItemTypes'
    ) as InternalOrganisationInvoiceItemTypeSettings[];

    this.invoiceItemTypeSettings = invoiceItemTypeSettingses.find(
      (v) => v.InvoiceItemType === this.object
    );

    this.salesAccount = this.invoiceItemTypeSettings?.SalesGeneralLedgerAccount;
    this.purchaseAccount =
      this.invoiceItemTypeSettings?.PurchaseGeneralLedgerAccount;
  }

  public onSalesAccountChange(event): void {
    const glAccount = event.source.value as GeneralLedgerAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.invoiceItemTypeSettings.SalesGeneralLedgerAccount = glAccount;
      }
    } else {
      if (event.source.selected) {
        this.invoiceItemTypeSettings.SalesGeneralLedgerAccount = glAccount;
      } else {
        this.invoiceItemTypeSettings.SalesGeneralLedgerAccount = null;
      }
    }
  }

  public onPurchaseAccountChange(event): void {
    const glAccount = event.source.value as GeneralLedgerAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.invoiceItemTypeSettings.PurchaseGeneralLedgerAccount = glAccount;
      }
    } else {
      if (event.source.selected) {
        this.invoiceItemTypeSettings.PurchaseGeneralLedgerAccount = glAccount;
      } else {
        this.invoiceItemTypeSettings.PurchaseGeneralLedgerAccount = null;
      }
    }
  }
}
