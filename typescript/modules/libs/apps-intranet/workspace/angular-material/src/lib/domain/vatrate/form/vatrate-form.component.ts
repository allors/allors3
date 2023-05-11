import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  GeneralLedgerAccount,
  InternalOrganisationVatRateSettings,
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
  glAccounts: GeneralLedgerAccount[];
  vatRateSettings: InternalOrganisationVatRateSettings;

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
        name: 'SettingsForVatRates',
        objectId: this.internalOrganisationId.value,
        select: {
          SettingsForAccounting: {
            SettingsForVatRates: {
              include: {
                VatRate: {},
                VatPayableAccount: {},
                VatReceivableAccount: {},
              },
            },
          },
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.VatRate({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.VatRegime({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.vatRegime = pullResult.object<VatRegime>(this.m.VatRegime);
    this.glAccounts = pullResult.collection<GeneralLedgerAccount>(
      this.m.GeneralLedgerAccount
    );

    const vatRateSettingses = pullResult.collection(
      'settingsForVatRates'
    ) as InternalOrganisationVatRateSettings[];

    this.vatRateSettings = vatRateSettingses.find(
      (v) => v.VatRate === this.object
    );

    if (this.createRequest) {
      this.vatRegime.addVatRate(this.object);
    }
  }
}
