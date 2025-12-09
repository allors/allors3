import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  DayOfWeek,
  RepeatingSalesInvoice,
  SalesInvoice,
  TimeFrequency,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './repeatingsalesinvoice-form.component.html',
  providers: [ContextService],
})
export class RepeatingSalesInvoiceFormComponent extends AllorsFormComponent<RepeatingSalesInvoice> {
  readonly m: M;
  frequencies: TimeFrequency[];
  daysOfWeek: DayOfWeek[];
  invoice: SalesInvoice;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
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
        p.RepeatingSalesInvoice({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Frequency: {},
            DayOfWeek: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(p.SalesInvoice({ objectId: initializer.id }));
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.frequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );

    this.invoice = pullResult.object<SalesInvoice>(this.m.SalesInvoice);
    this.daysOfWeek = pullResult.collection<DayOfWeek>(this.m.DayOfWeek);

    if (this.createRequest) {
      this.object.Source = this.invoice;
    }
  }
}
