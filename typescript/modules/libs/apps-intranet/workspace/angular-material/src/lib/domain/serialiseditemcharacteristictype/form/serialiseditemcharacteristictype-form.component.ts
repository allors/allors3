import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  IUnitOfMeasure,
  Locale,
  SerialisedItemCharacteristicType,
  Singleton,
  TimeFrequency,
  UnitOfMeasure,
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
  templateUrl: './serialiseditemcharacteristictype-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemCharacteristicTypeFormComponent extends AllorsFormComponent<SerialisedItemCharacteristicType> {
  public m: M;

  public uoms: IUnitOfMeasure[];
  public timeFrequencies: TimeFrequency[];
  public allUoms: IUnitOfMeasure[];
  locales: Locale[];

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
      this.fetcher.locales,
      p.UnitOfMeasure({
        predicate: {
          kind: 'Equals',
          propertyType: m.UnitOfMeasure.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.UnitOfMeasure.Name }],
      }),
      p.TimeFrequency({
        predicate: {
          kind: 'Equals',
          propertyType: m.TimeFrequency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.TimeFrequency.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.SerialisedItemCharacteristicType({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            LocalisedNames: {
              Locale: {},
            },
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

    this.uoms = pullResult.collection<UnitOfMeasure>(this.m.UnitOfMeasure);
    this.timeFrequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );
    this.allUoms = this.uoms
      .concat(this.timeFrequencies)
      .sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0));
    this.locales = this.fetcher.getAdditionalLocales(pullResult);

    if (this.createRequest) {
      this.object.IsActive = true;
    }
  }
}
