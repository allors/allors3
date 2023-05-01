import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  OrganisationGlAccount,
  VatRate,
  VatRegime,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './vatrate-form.component.html',
  providers: [ContextService],
})
export class VatRateFormComponent extends AllorsFormComponent<VatRate> {
  readonly m: M;
  vatRegime: VatRegime;
  vatPayableAccount: OrganisationGlAccount;
  vatToPayAccount: OrganisationGlAccount;
  vatReceivableAccount: OrganisationGlAccount;
  vatToReceiveAccount: OrganisationGlAccount;
  organisationGlAccounts: OrganisationGlAccount[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
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
        include: {
          InternalOrganisation: {},
          GeneralLedgerAccount: {},
        },
        predicate: {
          kind: 'Equals',
          propertyType: m.OrganisationGlAccount.InternalOrganisation,
          value: this.internalOrganisationId.value,
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.VatRate({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            VatPayableAccounts: { GeneralLedgerAccount: {} },
            VatToPayAccounts: { GeneralLedgerAccount: {} },
            VatReceivableAccounts: { GeneralLedgerAccount: {} },
            VatToReceiveAccounts: { GeneralLedgerAccount: {} },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.VatRegime({
          objectId: initializer.id,
          include: {
            VatRates: {
              VatPayableAccounts: { GeneralLedgerAccount: {} },
              VatToPayAccounts: { GeneralLedgerAccount: {} },
              VatReceivableAccounts: { GeneralLedgerAccount: {} },
              VatToReceiveAccounts: { GeneralLedgerAccount: {} },
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.vatRegime = pullResult.object<VatRegime>(this.m.VatRegime);
    this.organisationGlAccounts = pullResult.collection<OrganisationGlAccount>(
      this.m.OrganisationGlAccount
    );

    if (this.createRequest) {
      this.vatRegime.addVatRate(this.object);
    } else {
      this.vatPayableAccount = this.object.VatPayableAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.vatToPayAccount = this.object.VatToPayAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.vatReceivableAccount = this.object.VatReceivableAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.vatToReceiveAccount = this.object.VatToReceiveAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
    }
  }

  public onVatPayableAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addVatPayableAccount(organisationGlAccount);
        this.vatPayableAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addVatPayableAccount(organisationGlAccount);
      } else {
        this.object.removeVatPayableAccount(organisationGlAccount);
      }
    }
  }

  public onVatToPayAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addVatToPayAccount(organisationGlAccount);
        this.vatToPayAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addVatToPayAccount(organisationGlAccount);
      } else {
        this.object.removeVatToPayAccount(organisationGlAccount);
      }
    }
  }

  public onVatReceivableAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addVatReceivableAccount(organisationGlAccount);
        this.vatReceivableAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addVatReceivableAccount(organisationGlAccount);
      } else {
        this.object.removeVatReceivableAccount(organisationGlAccount);
      }
    }
  }

  public onVatToReceiveAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addVatToReceiveAccount(organisationGlAccount);
        this.vatToReceiveAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addVatToReceiveAccount(organisationGlAccount);
      } else {
        this.object.removeVatToReceiveAccount(organisationGlAccount);
      }
    }
  }
}
