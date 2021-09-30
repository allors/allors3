import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Model } from '@allors/workspace/domain/default';
import { SessionService } from '@allors/workspace/angular/core';

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

  constructor(private allors: SessionService) {
    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    this.model = this.allors.session.create<Model>(this.m.Model);
  }

  public ngOnDestroy(): void {
    if (this.model) {
      this.model.strategy.delete();
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
