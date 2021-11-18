import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Facility, InternalOrganisation, FacilityType } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';

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
  internalOrganisation: InternalOrganisation;

  constructor(
    private allors: ContextService,

    private fetcher: FetcherService
  ) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m; const { pullBuilder: pull } = m;

    const pulls = [
      this.fetcher.internalOrganisation,
      pull.Facility({}),
      pull.FacilityType({
        sorting: [{ roleType: this.m.FacilityType.Name }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
       this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
      this.facilities = loaded.collection<Facility>(m.Facility);

      this.facilityTypes = loaded.collection<FacilityType>(m.FacilityType);
      const storageLocation = this.facilityTypes?.find((v) => v.UniqueId === 'ff66c1ad-3048-48fd-a7d9-fbf97a090edd') ;

      this.facility = this.allors.context.create<Facility>(m.Facility);
      this.facility.Owner = this.internalOrganisation;
      this.facility.FacilityType = storageLocation;
    });
  }

  public ngOnDestroy(): void {
    if (this.facility) {
      this.facility.strategy.delete();
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
