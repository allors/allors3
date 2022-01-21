import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { Organisation } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
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

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    this.organisation = this.allors.context.create<Organisation>(
      this.m.Organisation
    );
  }

  public ngOnDestroy(): void {
    if (this.organisation) {
      this.organisation.strategy.delete();
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
