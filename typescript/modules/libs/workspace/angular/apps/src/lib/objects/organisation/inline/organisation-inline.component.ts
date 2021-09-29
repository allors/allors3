import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Organisation } from '@allors/workspace/domain/default';
import { SessionService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'organisation-inline',
  templateUrl: './organisation-inline.component.html',
})
export class OrganisationInlineComponent implements OnInit, OnDestroy {
  @Output()
  public saved: EventEmitter<Organisation> = new EventEmitter<Organisation>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public organisation: Organisation;

  public m: M;

  constructor(private allors: SessionService) {
    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    this.organisation = this.allors.session.create<Organisation>(this.m.Organisation);
  }

  public ngOnDestroy(): void {
    if (this.organisation) {
      this.allors.client.invokeReactive(this.allors.session, this.organisation.Delete);
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.organisation);
    this.organisation = undefined;
  }
}
