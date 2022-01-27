import { Component, Inject, ViewChild, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CountryFormComponent } from '../forms/country-form.component';
import { IObject } from '@allors/system/workspace/domain';
import {
  CreateRequest,
  EditRequest,
} from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: 'country-edit.component.html',
})
export class CountryEditComponent implements OnInit {
  @ViewChild(CountryFormComponent)
  private country!: CountryFormComponent;

  constructor(
    @Inject(MAT_DIALOG_DATA) private request: CreateRequest | EditRequest,
    private dialogRef: MatDialogRef<CountryEditComponent>
  ) {}

  ngOnInit(): void {
    if (this.request.kind === 'EditRequest')
      this.country.edit(this.request.object.id);
  }

  saved(object: IObject) {
    this.dialogRef.close(object);
  }

  cancelled() {
    this.dialogRef.close();
  }
}
