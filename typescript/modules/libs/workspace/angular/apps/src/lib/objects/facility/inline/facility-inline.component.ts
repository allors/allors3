import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { SessionService, MetaService } from '@allors/angular/services/core';
import { Facility, FacilityType, Organisation } from '@allors/domain/generated';
import { Meta } from '@allors/meta/generated';
import { Sort } from '@allors/data/system';
import { PullRequest } from '@allors/protocol/system';
import { FetcherService } from '@allors/angular/base';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'facility-inline',
  templateUrl: './facility-inline.component.html',
})
export class FacilityInlineComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<Facility> = new EventEmitter<Facility>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public m: M;

  facilityTypes: FacilityType[];
  public facility: Facility;

  facilities: Facility[];
  internalOrganisation: Organisation;

  constructor(
    private allors: SessionService,
    
    private fetcher: FetcherService
  ) {

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const { pullBuilder: pull } = this.m;

    const pulls = [
      this.fetcher.internalOrganisation,
      pull.Facility(),
      pull.FacilityType({
        sorting: [{ roleType: this.m.FacilityType.Name }]
      })
    ];

    this.allors.client.pullReactive(this.allors.session, pulls)
      .subscribe((loaded) => {
        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);
        this.facilities = loaded.collection<Facility>(m.Facility);

        this.facilityTypes = loaded.collection<FacilityType>(m.FacilityType);
        const storageLocation = this.facilityTypes.find((v) => v.UniqueId === 'ff66c1ad-3048-48fd-a7d9-fbf97a090edd');

        this.facility = this.allors.session.create<Facility>(m.Facility);
        this.facility.Owner = this.internalOrganisation;
        this.facility.FacilityType = storageLocation;
      });
  }

  public ngOnDestroy(): void {
    if (!!this.facility) {
      this.allors.client.invokeReactive(this.allors.session, this.facility.Delete);
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.facility);
    this.facility = undefined;
  }
}
