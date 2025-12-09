import { timer } from 'rxjs';
import {
  Component,
  Inject,
  ViewChild,
  OnInit,
  ElementRef,
} from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatStepper } from '@angular/material/stepper';
import { assert } from '@allors/system/workspace/meta';
import { parameterizablePredicateObjectType } from '@allors/system/workspace/domain';
import {
  AllorsComponent,
  Filter,
  FilterField,
  FilterFieldDefinition,
} from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: 'dialog.component.html',
})
export class AllorsMaterialFilterFieldDialogComponent
  extends AllorsComponent
  implements OnInit
{
  @ViewChild('stepper', { static: true }) stepper: MatStepper;
  @ViewChild('focus1') focus1: ElementRef;
  @ViewChild('focus2') focus2: ElementRef;
  @ViewChild('focus3') focus3: ElementRef;
  @ViewChild('focus4') focus4: ElementRef;

  formGroup: FormGroup;
  filter: Filter;
  fieldDefinition: FilterFieldDefinition | null;

  constructor(
    public dialogRef: MatDialogRef<AllorsMaterialFilterFieldDialogComponent>,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) data: any
  ) {
    super();
    this.filter = data.filterBuilder;
  }

  get isBetween() {
    return this.fieldDefinition?.isBetween ?? false;
  }

  get placeholder() {
    return this.isBetween ? 'From' : 'Value';
  }

  get useSearch(): boolean {
    return !!this.fieldDefinition?.options?.search;
  }

  get useToggle(): boolean {
    parameterizablePredicateObjectType(this.fieldDefinition?.predicate);
    return (
      !this.fieldDefinition?.options?.search &&
      (this.fieldDefinition?.isBoolean ?? false)
    );
  }

  get useDatepicker(): boolean {
    return (
      !this.fieldDefinition?.options?.search &&
      (this.fieldDefinition?.isDateTime ?? false)
    );
  }

  get useInput(): boolean {
    return (
      !this.fieldDefinition?.options?.search &&
      (!this.fieldDefinition?.isBinary ?? false) &&
      (!this.fieldDefinition?.isDateTime ?? false)
    );
  }

  ngOnInit() {
    this.formGroup = this.formBuilder.group({
      definition: ['', Validators.required],
      value: ['', Validators.required],
      value2: ['', Validators.required],
    });
  }

  focus() {
    setTimeout(() => {
      this.focus1?.nativeElement?.focus();
      this.focus2?.nativeElement?.focus();
      this.focus3?.nativeElement?.focus();
      this.focus4?.nativeElement?.focus();
    }, 0);
  }

  stepperSelectionChange(event: StepperSelectionEvent) {
    if (event.selectedIndex === 0) {
      this.fieldDefinition = null;
    }
  }

  selected(filterFieldDefinition: FilterFieldDefinition) {
    this.fieldDefinition = filterFieldDefinition;
    let initialValue = null;

    const initial = filterFieldDefinition.options?.initialValue;
    if (initial != null) {
      initialValue = initial instanceof Function ? initial() : initial;
      if (filterFieldDefinition.isBoolean) {
        initialValue = true;
      }
    }

    const valueControl = this.formGroup.get('value');
    assert(valueControl);
    valueControl.setValue(initialValue);

    // give angular time to process the [completed] directive
    timer(1).subscribe(() => this.stepper.next());
  }

  apply() {
    assert(this.fieldDefinition);

    const objectType = this.fieldDefinition.objectType;
    const options = this.fieldDefinition.options;

    const value = this.formGroup.get('value')?.value;
    const value2 = this.formGroup.get('value2')?.value;

    const inValid =
      value == null || (objectType.isComposite && value.strategy == null);

    if (!inValid) {
      if (!value2) {
        this.filter.addField(
          new FilterField({
            definition: this.fieldDefinition,
            value: value.id ? value.id : value,
            display: options?.display(value) ?? value,
          })
        );
      } else {
        this.filter.addField(
          new FilterField({
            definition: this.fieldDefinition,
            value,
            value2,
            display: options?.display
              ? `${options.display(value)} <-> ${options.display(value2)}`
              : `${value} <-> ${value2}`,
          })
        );
      }
    }

    this.dialogRef.close();
  }
}
