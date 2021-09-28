import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { TestScope } from '@allors/angular/core';
import { Carrier, SerialisedItemCharacteristicType } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { IObject } from '@allors/domain/system';
import { Meta } from '@allors/meta/generated';
import { MetaService, SessionService, RefreshService } from '@allors/angular/services/core';
import { ObjectData, SaveService } from '@allors/angular/material/services/core';

@Component({
  templateUrl: './carrier-edit.component.html',
  providers: [SessionService]
})
export class CarrierEditComponent extends TestScope implements OnInit, OnDestroy {

  public title: string;
  public subTitle: string;

  public m: M;

  public carrier: Carrier;

  public characteristics: SerialisedItemCharacteristicType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CarrierEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
  ) {

    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const { pull } = this.metaService;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          const pulls = [
          ];

          if (!isCreate) {
            pulls.push(
              pull.Carrier({
                objectId: this.data.id,
              }),
            );
          }

          return this.allors.context
            .load(new PullRequest({ pulls }))
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {

        this.allors.session.reset();

        this.characteristics = loaded.collection<SerialisedItemCharacteristicType>(m.SerialisedItemCharacteristicType);

        if (isCreate) {
          this.title = 'Add Carrier';
          this.carrier = this.allors.session.create<Carrier>(m.Carrier);
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

    this.allors.context
      .save()
      .subscribe(() => {
        const data: IObject = {
          id: this.carrier.id,
          objectType: this.carrier.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
