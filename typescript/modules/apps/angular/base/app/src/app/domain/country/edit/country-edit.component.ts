import { Component, Inject, AfterViewInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  CreateDialogData,
  EditDialogData,
} from '@allors/workspace/angular-material/base';
import { CountryFormComponent } from '../forms/country-form.component';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: 'country-edit.component.html',
})
export class CountryEditComponent implements AfterViewInit {
  @ViewChild(CountryFormComponent)
  private country!: CountryFormComponent;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: CreateDialogData | EditDialogData,
    private dialogRef: MatDialogRef<CountryEditComponent>
  ) {}

  ngAfterViewInit(): void {
    if (this.data.kind === 'EditDialogData')
      this.country.edit(this.data.object.id);
  }

  saved(object: IObject) {
    this.dialogRef.close(object);
  }

  cancelled() {
    this.dialogRef.close();
  }
}
