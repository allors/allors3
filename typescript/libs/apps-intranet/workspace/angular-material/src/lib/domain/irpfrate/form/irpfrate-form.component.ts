import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  GeneralLedgerAccount,
  InternalOrganisation,
  InternalOrganisationIrpfRateSettings,
  IrpfRate,
  IrpfRegime,
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
  glAccounts: GeneralLedgerAccount[];
  irpfRateSettings: InternalOrganisationIrpfRateSettings;
  applicable: boolean;

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
        name: 'SettingsForIrpfRates',
        objectId: this.internalOrganisationId.value,
        select: {
          SettingsForAccounting: {
            SettingsForIrpfRates: {
              include: {
                IrpfRate: {},
                IrpfPayableAccount: {},
                IrpfReceivableAccount: {},
              },
            },
          },
        },
      }),
      p.InternalOrganisation({
        objectId: this.internalOrganisationId.value,
        include: {
          Country: {},
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.IrpfRate({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.IrpfRegime({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    const internalOrganisation = pullResult.object<InternalOrganisation>(
      this.m.InternalOrganisation
    );

    this.applicable = internalOrganisation.Country.IsoCode == 'ES';

    this.irpfRegime = pullResult.object<IrpfRegime>(this.m.IrpfRegime);
    this.glAccounts = pullResult.collection<GeneralLedgerAccount>(
      this.m.GeneralLedgerAccount
    );

    const irpfRateSettingses = pullResult.collection(
      'settingsForIrpfRates'
    ) as InternalOrganisationIrpfRateSettings[];

    this.irpfRateSettings = irpfRateSettingses.find(
      (v) => v.IrpfRate === this.object
    );

    if (this.createRequest) {
      this.irpfRegime.addIrpfRate(this.object);
    }
  }
}
