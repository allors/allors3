import { Component, OnDestroy, OnInit, Output, EventEmitter } from '@angular/core';

import { M } from '@allors/workspace/meta/default';
import { Brand } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'brand-inline',
  templateUrl: './brand-inline.component.html',
})
export class InlineBrandComponent implements OnInit, OnDestroy {
  @Output() public saved: EventEmitter<Brand> = new EventEmitter<Brand>();

  @Output() public cancelled: EventEmitter<any> = new EventEmitter();

  public brand: Brand;

  public m: M;

  constructor(private allors: ContextService) {
    this.m = allors.context.configuration.metaPopulation as M;
  }

  ngOnInit(): void {
    this.brand = this.allors.context.create<Brand>(this.m.Brand);
  }

  public ngOnDestroy(): void {
    if (this.brand) {
      this.brand.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.brand);
    this.brand = undefined;
  }
}
