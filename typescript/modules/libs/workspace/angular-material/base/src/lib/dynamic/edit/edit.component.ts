import { Component, Inject, AfterViewInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { IObject } from '@allors/workspace/domain/system';
import {
  AllorsForm,
  angularForms,
  ObjectData,
} from '@allors/workspace/angular/base';
import { DynamicFormHostDirective } from '../form/form-host.directive';

@Component({
  templateUrl: 'edit.component.html',
})
export class DynamicEditComponent implements AfterViewInit {
  @ViewChild(DynamicFormHostDirective, { static: true })
  dynamicFormHost!: DynamicFormHostDirective;

  component: unknown;

  form: AllorsForm;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: ObjectData,
    private dialogRef: MatDialogRef<DynamicEditComponent>
  ) {}

  ngAfterViewInit(): void {
    const viewContainerRef = this.dynamicFormHost.viewContainerRef;
    viewContainerRef.clear();

    if (this.data.id) {
      const componentRef = viewContainerRef.createComponent<AllorsForm>(
        angularForms(this.data.associationObjectType).create
      );

      this.form = componentRef.instance;
      this.form.edit(this.data.id);
    } else {
      const objectType = this.data.strategy.cls;
      const component = angularForms(objectType).create;
      const componentRef =
        viewContainerRef.createComponent<AllorsForm>(component);

      this.form = componentRef.instance;
      this.form.create(objectType);
    }
  }

  saved(object: IObject) {
    this.dialogRef.close(object);
  }

  cancelled() {
    this.dialogRef.close();
  }
}
