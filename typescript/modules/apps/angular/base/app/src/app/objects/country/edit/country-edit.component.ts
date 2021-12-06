import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Country } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  templateUrl: './country-edit.component.html',
  providers: [ContextService],
})
export class CountryEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public country: Country;

  private subscription: Subscription;

  constructor(@Self() public allors: ContextService, @Inject(MAT_DIALOG_DATA) public data: ObjectData, public dialogRef: MatDialogRef<CountryEditComponent>, public refreshService: RefreshService, private saveService: SaveService) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.Country({
                objectId: this.data.id,
              })
            );
          }

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        if (isCreate) {
          this.title = 'Add Country';
          this.country = this.allors.context.create<Country>(m.Country);
        } else {
          this.country = loaded.object<Country>(m.Country);

          if (this.country.canWriteName) {
            this.title = 'Edit Country';
          } else {
            this.title = 'View Country';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.country);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
