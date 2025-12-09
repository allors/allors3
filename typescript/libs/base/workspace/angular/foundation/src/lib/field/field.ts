import {
  AfterViewInit,
  Directive,
  OnDestroy,
  QueryList,
  ViewChildren,
} from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';
import { Subscription, tap } from 'rxjs';
import { AllorsComponent } from '../component';

@Directive()
export abstract class Field
  extends AllorsComponent
  implements AfterViewInit, OnDestroy
{
  protected static counter = 0;

  override dataAllorsKind = 'field';

  private previousControls: Set<NgModel> = new Set();
  private controlSubscription: Subscription;

  @ViewChildren(NgModel) private controls: QueryList<NgModel>;

  constructor(protected form: NgForm) {
    super();
  }

  ngAfterViewInit(): void {
    this.syncControls(new Set(this.controls.toArray()));

    this.controlSubscription = this.controls.changes
      .pipe(
        tap((controls) => {
          this.syncControls(new Set(controls.toArray()));
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.syncControls(new Set());
    this.controlSubscription?.unsubscribe();
  }

  private syncControls(newControls: Set<NgModel>) {
    if (this.form) {
      this.previousControls.forEach((control: NgModel) => {
        if (!newControls.has(control)) {
          this.form.removeControl(control);
        }
      });

      newControls.forEach((control: NgModel) => {
        if (!this.previousControls.has(control)) {
          this.form.addControl(control);
        }
      });
    }

    this.previousControls = newControls;
  }
}
