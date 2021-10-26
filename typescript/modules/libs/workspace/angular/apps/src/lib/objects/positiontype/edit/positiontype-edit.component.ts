import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { PositionType } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './positiontype-edit.component.html',
  providers: [ContextService],
})
export class PositionTypeEditComponent extends TestScope implements OnInit, OnDestroy {
  public title: string;
  public subTitle: string;

  public m: M;

  public positionType: PositionType;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<PositionTypeEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m; const { pullBuilder: pull } = m;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.PositionType({
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
          this.title = 'Add Position Type';
          this.positionType = this.allors.context.create<PositionType>(m.PositionType);
        } else {
          this.positionType = loaded.object<PositionType>(m.PositionType);

          if (this.positionType.canWriteTitle) {
            this.title = 'Edit Position Type';
          } else {
            this.title = 'View Position Type';
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
      this.dialogRef.close(this.positionType);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
