import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Facility,
  FacilityType,
  Organisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './facility-form.component.html',
  providers: [ContextService],
})
export class FacilityFormComponent extends AllorsFormComponent<Facility> {
  m: M;
  facilityTypes: FacilityType[];
  owners: Organisation[];
  parents: Facility[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.locales);

    pulls.push(
      p.Facility({
        sorting: [{ roleType: m.Facility.Name }],
      }),
      p.FacilityType({
        predicate: {
          kind: 'Equals',
          propertyType: m.FacilityType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.FacilityType.Name }],
      }),
      p.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.Facility({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Owner: {},
            FacilityType: {},
            ParentFacility: {},
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

    this.facilityTypes = pullResult.collection<FacilityType>(
      this.m.FacilityType
    );

    this.owners = pullResult.collection<Organisation>(this.m.Organisation);
    this.parents = pullResult.collection<Facility>(this.m.Facility);

    this.onPostPullInitialize(pullResult);
  }
}
