import { Component, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormService, ObjectData } from '@allors/workspace/angular/base';

@Component({
  templateUrl: 'country-edit.component.html',
  providers: [FormService],
})
export class CountryEditComponent {
  constructor(
    @Self() formService: FormService,
    @Inject(MAT_DIALOG_DATA) data: ObjectData,
    private dialogRef: MatDialogRef<CountryEditComponent>
  ) {
    formService.cancelled$.subscribe(() => this.dialogRef.close());
    formService.saved$.subscribe((object) => this.dialogRef.close(object));

    if (data.id) {
      formService.edit(data.id);
    }
  }
}
