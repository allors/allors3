import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { SessionService, MetaService } from '@allors/angular/services/core';
import { Model } from '@allors/domain/generated';
import { Meta } from '@allors/meta/generated';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'model-inline',
  templateUrl: './model-inline.component.html',
})
export class InlineModelComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<Model> = new EventEmitter<Model>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public model: Model;

  public m: M;

  constructor(
    private allors: SessionService,
    
    ) {

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    this.model = this.allors.context.create('Model') as Model;
  }

  public ngOnDestroy(): void {
    if (!!this.model) {
      this.allors.context.delete(this.model);
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
      this.saved.emit(this.model);
      this.model = undefined;
  }
}
