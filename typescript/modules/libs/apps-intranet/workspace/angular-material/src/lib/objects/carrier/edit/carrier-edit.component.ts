import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Carrier,
  SerialisedItemCharacteristicType,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './carrier-edit.component.html',
  providers: [ContextService],
})
export class CarrierEditComponent implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public carrier: Carrier;

  public characteristics: SerialisedItemCharacteristicType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CarrierEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
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
              pull.Carrier({
                objectId: this.data.id,
              })
            );
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.characteristics =
          loaded.collection<SerialisedItemCharacteristicType>(
            m.SerialisedItemCharacteristicType
          );

        if (isCreate) {
          this.title = 'Add Carrier';
          this.carrier = this.allors.context.create<Carrier>(m.Carrier);
        } else {
          this.carrier = loaded.object<Carrier>(m.Carrier);

          if (this.carrier.canWriteName) {
            this.title = 'Edit Carrier';
          } else {
            this.title = 'View Carrier';
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
      this.dialogRef.close(this.carrier);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
