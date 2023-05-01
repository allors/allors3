import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  IrpfRate,
  IrpfRegime,
  OrganisationGlAccount,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './irpfrate-form.component.html',
  providers: [ContextService],
})
export class IrpfRateFormComponent extends AllorsFormComponent<IrpfRate> {
  readonly m: M;
  irpfRegime: IrpfRegime;
  irpfPayableAccount: OrganisationGlAccount;
  irpfToPayAccount: OrganisationGlAccount;
  irpfReceivableAccount: OrganisationGlAccount;
  irpfToReceiveAccount: OrganisationGlAccount;
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
        p.IrpfRate({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            IrpfPayableAccounts: { GeneralLedgerAccount: {} },
            IrpfToPayAccounts: { GeneralLedgerAccount: {} },
            IrpfReceivableAccounts: { GeneralLedgerAccount: {} },
            IrpfToReceiveAccounts: { GeneralLedgerAccount: {} },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.IrpfRegime({
          objectId: initializer.id,
          include: {
            IrpfRates: {
              IrpfPayableAccounts: { GeneralLedgerAccount: {} },
              IrpfToPayAccounts: { GeneralLedgerAccount: {} },
              IrpfReceivableAccounts: { GeneralLedgerAccount: {} },
              IrpfToReceiveAccounts: { GeneralLedgerAccount: {} },
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

    this.irpfRegime = pullResult.object<IrpfRegime>(this.m.IrpfRegime);
    this.organisationGlAccounts = pullResult.collection<OrganisationGlAccount>(
      this.m.OrganisationGlAccount
    );

    if (this.createRequest) {
      this.irpfRegime.addIrpfRate(this.object);
    } else {
      this.irpfPayableAccount = this.object.IrpfPayableAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.irpfToPayAccount = this.object.IrpfToPayAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.irpfReceivableAccount = this.object.IrpfReceivableAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
      this.irpfToReceiveAccount = this.object.IrpfToReceiveAccounts.find(
        (v) => v.InternalOrganisation.id === this.internalOrganisationId.value
      );
    }
  }

  public onIrpfPayableAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addIrpfPayableAccount(organisationGlAccount);
        this.irpfPayableAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addIrpfPayableAccount(organisationGlAccount);
      } else {
        this.object.removeIrpfPayableAccount(organisationGlAccount);
      }
    }
  }

  public onIrpfToPayAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addIrpfToPayAccount(organisationGlAccount);
        this.irpfToPayAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addIrpfToPayAccount(organisationGlAccount);
      } else {
        this.object.removeIrpfToPayAccount(organisationGlAccount);
      }
    }
  }

  public onIrpfReceivableAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addIrpfReceivableAccount(organisationGlAccount);
        this.irpfReceivableAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addIrpfReceivableAccount(organisationGlAccount);
      } else {
        this.object.removeIrpfReceivableAccount(organisationGlAccount);
      }
    }
  }

  public onIrpfToReceiveAccountChange(event): void {
    const organisationGlAccount = event.source.value as OrganisationGlAccount;

    if (event.isUserInput) {
      if (event.source.selected) {
        this.object.addIrpfToReceiveAccount(organisationGlAccount);
        this.irpfToReceiveAccount = organisationGlAccount;
      }
    } else {
      if (event.source.selected) {
        this.object.addIrpfToReceiveAccount(organisationGlAccount);
      } else {
        this.object.removeIrpfToReceiveAccount(organisationGlAccount);
      }
    }
  }
}
