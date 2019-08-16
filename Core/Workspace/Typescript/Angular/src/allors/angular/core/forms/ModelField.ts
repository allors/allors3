// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { AfterViewInit, Input, OnDestroy, QueryList, ViewChildren, Directive } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';
import { ISessionObject, } from '../../../framework';
import { Field } from './Field';

// See https://github.com/angular/angular/issues/30080
@Directive({selector: 'ivy-workaround-model-field'})
export abstract class ModelField extends Field implements AfterViewInit, OnDestroy {

  @Input()
  public model: ISessionObject;

  @Input()
  public name: string;

  @Input()
  public disabled: boolean;

  @Input()
  public required: boolean;

  @Input()
  public label: string;

  @Input()
  public readonly: boolean;

  @Input()
  public hint: string;

  @Input()
  public focus: boolean;

  @ViewChildren(NgModel) private controls: QueryList<NgModel>;

  private id = 0;

  constructor(private parentForm: NgForm) {
    super();
    // TODO: wrap around
    this.id = ++Field.counter;
  }

  public ngAfterViewInit(): void {
    if (!!this.parentForm) {
      this.controls.forEach((control: NgModel) => {
        this.parentForm.addControl(control);
      });
    }
  }

  public ngOnDestroy(): void {
    if (!!this.parentForm) {
      this.controls.forEach((control: NgModel) => {
        this.parentForm.removeControl(control);
      });
    }
  }

  get dataAllorsId(): string {
    return this.model ? this.model.id : null;
  }
}