import { Component, Inject, AfterViewInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ObjectData } from '@allors/workspace/angular/base';
import { CountryFormComponent } from '../forms/country-form.component';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: 'country-edit.component.html',
})
export class CountryEditComponent implements AfterViewInit {
  @ViewChild(CountryFormComponent)
  private country!: CountryFormComponent;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: ObjectData,
    private dialogRef: MatDialogRef<CountryEditComponent>
  ) {}

  ngAfterViewInit(): void {
    if (this.data.id) {
      this.country.edit(this.data.id);
    }
  }

  saved(object: IObject) {
    this.dialogRef.close(object);
  }

  cancelled() {
    this.dialogRef.close();
  }
}
