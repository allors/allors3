import { Component, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { Country } from '@allors/workspace/domain/default';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { AllorsEditComponent } from '@allors/workspace/angular-material/base';

@Component({
  templateUrl: 'country-edit.component.html',
  providers: [ContextService],
})
export class CountryEditComponent extends AllorsEditComponent<
  Country,
  CountryEditComponent
> {
  constructor(
    @Self() allors: ContextService,
    @Inject(MAT_DIALOG_DATA) data: ObjectData,
    dialogRef: MatDialogRef<CountryEditComponent>,
    refreshService: RefreshService,
    saveService: SaveService
  ) {
    super(allors, data, dialogRef, refreshService, saveService);
  }

  get canWrite() {
    return this.object?.canWriteName ?? true;
  }

  onPull(pulls: Pull[]) {
    const { pullBuilder: p } = this.m;

    if (!this.isCreate) {
      pulls.push(
        p.Country({
          objectId: this.data.id,
        })
      );
    }
  }

  onPulled(loaded: IPullResult) {
    this.allors.context.reset();

    const { m } = this;
    if (this.isCreate) {
      this.object = this.allors.context.create<Country>(m.Country);
    } else {
      this.object = loaded.object<Country>(m.Country);
    }
  }
}
