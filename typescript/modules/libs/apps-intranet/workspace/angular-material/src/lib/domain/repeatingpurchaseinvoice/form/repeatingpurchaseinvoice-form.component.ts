import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  DayOfWeek,
  Organisation,
  RepeatingPurchaseInvoice,
  TimeFrequency,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './repeatingpurchaseinvoice-form.component.html',
  providers: [ContextService],
})
export class RepeatingPurchaseInvoiceFormComponent extends AllorsFormComponent<RepeatingPurchaseInvoice> {
  readonly m: M;

  frequencies: TimeFrequency[];
  daysOfWeek: DayOfWeek[];
  supplier: Organisation;
  internalOrganisations: Organisation[];

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
      p.Organisation({
        name: 'InternalOrganisations',
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
      }),
      p.TimeFrequency({
        predicate: {
          kind: 'Equals',
          propertyType: m.TimeFrequency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.TimeFrequency.Name }],
      }),
      p.DayOfWeek({})
    );

    if (this.editRequest) {
      pulls.push(
        p.RepeatingPurchaseInvoice({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Frequency: {},
            DayOfWeek: {},
          },
        })
      );
    }

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(p.Organisation({ objectId: initializer.id }));
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.supplier = pullResult.object<Organisation>(this.m.Organisation);
    this.frequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );
    this.daysOfWeek = pullResult.collection<DayOfWeek>(this.m.DayOfWeek);
    this.internalOrganisations = pullResult.collection<Organisation>(
      'InternalOrganisations'
    );

    if (this.createRequest) {
      this.object.Supplier = this.supplier;
    }
  }
}
