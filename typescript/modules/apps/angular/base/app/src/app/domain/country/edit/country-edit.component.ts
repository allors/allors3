import { combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Country } from '@allors/workspace/domain/default';
import { ContextService } from '@allors/workspace/angular/core';
import {
  AllorsEditComponent,
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';

@Component({
  template: `
    <a-form *ngIf="object" (submit)="save()">
      <h3 mat-dialog-title>{{ title }}</h3>

      <mat-dialog-content>
        <div class="row">
          <a-mat-input
            class="col-md"
            [object]="object"
            [roleType]="m.Country.IsoCode"
          ></a-mat-input>
          <a-mat-input
            class="col-md"
            [object]="object"
            [roleType]="m.Country.Name"
          ></a-mat-input>
        </div>
      </mat-dialog-content>

      <div mat-dialog-actions>
        <div mat-dialog-actions>
          <a-mat-cancel (cancel)="dialogRef.close()"></a-mat-cancel>
          <a-mat-save></a-mat-save>
        </div>
      </div>
    </a-form>
  `,
  providers: [ContextService],
})
export class CountryEditComponent
  extends AllorsEditComponent<Country, CountryEditComponent>
  implements OnInit, OnDestroy
{
  constructor(
    @Self() allors: ContextService,
    @Inject(MAT_DIALOG_DATA) data: ObjectData,
    dialogRef: MatDialogRef<CountryEditComponent>,
    refreshService: RefreshService,
    saveService: SaveService
  ) {
    super(allors, data, dialogRef, refreshService, saveService);
  }

  get canEdit() {
    return this.object?.canWriteName ?? true;
  }

  public ngOnInit(): void {
    const { pullBuilder: pull } = this.m;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const pulls = [];

          if (!this.isCreate) {
            pulls.push(
              pull.Country({
                objectId: this.data.id,
              })
            );
          }

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const { m } = this;
        if (this.isCreate) {
          this.object = this.allors.context.create<Country>(m.Country);
        } else {
          this.object = loaded.object<Country>(m.Country);
        }
      });
  }
}
