import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  PositionType,
  PositionTypeRate,
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
  templateUrl: './positiontyperate-form.component.html',
  providers: [ContextService],
})
export class PositionTypeRateFormComponent extends AllorsFormComponent<PositionTypeRate> {
  readonly m: M;

  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];
  selectedPositionTypes: PositionType[];

  positionTypes: PositionType[];
  originalPositionTypes: PositionType[];

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
      }),
      p.PositionType({
        sorting: [{ roleType: this.m.PositionType.Title }],
        include: {
          PositionTypeRate: {},
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.PositionTypeRate({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            RateType: {},
            Frequency: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.rateTypes = pullResult.collection<RateType>(this.m.RateType);
    this.timeFrequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );
    const hour = this.timeFrequencies?.find(
      (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
    );

    this.positionTypes = pullResult.collection<PositionType>(
      this.m.PositionType
    );
    this.selectedPositionTypes = this.positionTypes?.filter(
      (v) => v.PositionTypeRate === this.object
    );
    this.originalPositionTypes = this.selectedPositionTypes;

    if (this.createRequest) {
      this.object.Frequency = hour;
    }
  }

  public override save(): void {
    if (this.selectedPositionTypes != null) {
      this.selectedPositionTypes.forEach((positionType: PositionType) => {
        positionType.PositionTypeRate = this.object;

        const index = this.originalPositionTypes.indexOf(positionType);
        if (index > -1) {
          this.originalPositionTypes.splice(index, 1);
        }
      });
    }

    this.originalPositionTypes.forEach((positionType: PositionType) => {
      positionType.PositionTypeRate = null;
    });

    super.save();
  }
}
