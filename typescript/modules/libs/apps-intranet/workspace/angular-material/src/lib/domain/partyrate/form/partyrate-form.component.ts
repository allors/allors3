import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Party,
  PartyRate,
  RateType,
  TimeFrequency,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './partyrate-form.component.html',
  providers: [ContextService],
})
export class PartyRateFormComponent extends AllorsFormComponent<PartyRate> {
  readonly m: M;
  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];
  party: Party;

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
      p.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
      p.TimeFrequency({
        sorting: [{ roleType: this.m.TimeFrequency.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.PartyRate({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            RateType: {},
            Frequency: {},
          },
        })
      );
    }

    const initializer = this.createRequest.initializer;
    if (initializer) {
      pulls.push(
        p.Party({
          objectId: initializer.id,
          include: {
            PartyRates: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.party = pullResult.object<Party>(this.m.Party);
    this.rateTypes = pullResult.collection<RateType>(this.m.RateType);
    this.timeFrequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );
    const hour = this.timeFrequencies?.find(
      (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
    );

    if (this.createRequest) {
      this.object.Frequency = hour;
      this.party.addPartyRate(this.object);
    }
  }
}
