import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PositionTypeRate, TimeFrequency, RateType, PositionType } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './positiontyperate-edit.component.html',
  providers: [SessionService],
})
export class PositionTypeRateEditComponent extends TestScope implements OnInit, OnDestroy {
  title: string;
  subTitle: string;

  readonly m: M;

  positionTypeRate: PositionTypeRate;
  timeFrequencies: TimeFrequency[];
  rateTypes: RateType[];
  selectedPositionTypes: PositionType[];

  private subscription: Subscription;
  positionTypes: PositionType[];
  originalPositionTypes: PositionType[];

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PositionTypeRateEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
            pull.TimeFrequency({ sorting: [{ roleType: this.m.TimeFrequency.Name }] }),
            pull.PositionType({
              sorting: [{ roleType: this.m.PositionType.Title }],
              include: {
                PositionTypeRate: x,
              },
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.PositionTypeRate({
                objectId: this.data.id,
                include: {
                  RateType: x,
                  Frequency: x,
                },
              })
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.rateTypes = loaded.collection<RateType>(m.RateType);
        this.timeFrequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        const hour = this.timeFrequencies.find((v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5');

        if (isCreate) {
          this.title = 'Add Position Type Rate';
          this.positionTypeRate = this.allors.session.create<PositionTypeRate>(m.PositionTypeRate);
          this.positionTypeRate.Frequency = hour;
        } else {
          this.positionTypeRate = loaded.object<PositionTypeRate>(m.PositionTypeRate);

          if (this.positionTypeRate.canWriteRate) {
            this.title = 'Edit Position Type Rate';
          } else {
            this.title = 'View Position Type Rate';
          }
        }

        this.positionTypes = loaded.collection<PositionType>(m.PositionType);
        this.selectedPositionTypes = this.positionTypes.filter((v) => v.PositionTypeRate === this.positionTypeRate);
        this.originalPositionTypes = this.selectedPositionTypes;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public setDirty(): void {
    this.allors.session.hasChanges = true;
  }

  public save(): void {
    if (this.selectedPositionTypes !== undefined) {
      this.selectedPositionTypes.forEach((positionType: PositionType) => {
        positionType.PositionTypeRate = this.positionTypeRate;

        const index = this.originalPositionTypes.indexOf(positionType);
        if (index > -1) {
          this.originalPositionTypes.splice(index, 1);
        }
      });
    }

    this.originalPositionTypes.forEach((positionType: PositionType) => {
      positionType.PositionTypeRate = null;
    });

    this.allors.context.save().subscribe(() => {
      const data: IObject = {
        id: this.positionTypeRate.id,
        objectType: this.positionTypeRate.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
