import { Component, Inject, ViewChild, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CountryFormComponent } from '../forms/country-form.component';
import { IObject } from '@allors/system/workspace/domain';
import {
  CreateData,
  EditData,
} from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: 'country-edit.component.html',
})
export class CountryEditComponent implements OnInit {
  @ViewChild(CountryFormComponent)
  private country!: CountryFormComponent;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: CreateData | EditData,
    private dialogRef: MatDialogRef<CountryEditComponent>
  ) {}

  ngOnInit(): void {
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
