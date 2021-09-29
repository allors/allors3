import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { OrderAdjustment } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/workspace/domain/system';

@Component({
  templateUrl: './orderadjustment-edit.component.html',
  providers: [SessionService],
})
export class OrderAdjustmentEditComponent extends TestScope implements OnInit, OnDestroy {
  public m: M;

  public title: string;

  public container: IObject;
  public object: OrderAdjustment;

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<OrderAdjustmentEditComponent>,

    public refreshService: RefreshService,
    private saveService: SaveService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m; const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = (this.data as IObject).id === undefined;
          const { strategy: { cls }, associationRoleType } = this.data;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.OrderAdjustment({
                objectId: this.data.id,
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(pull.Quote({ objectId: this.data.associationId }), pull.Order({ objectId: this.data.associationId }), pull.Invoice({ objectId: this.data.associationId }));
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, create: isCreate, cls, associationRoleType })));
        })
      )
      .subscribe(({ loaded, create, cls, associationRoleType }) => {
        this.allors.session.reset();

        this.container = loaded.objects.Quote || loaded.objects.Order || loaded.objects.Invoice;
        this.object = loaded.object<OrderAdjustment>(m.OrderAdjustment);

        if (create) {
          this.title = `Add ${cls.singularName}`;
          this.object = this.allors.session.create<OrderAdjustment>(cls);
          this.container.strategy.addCompositesRole(associationRoleType, this.object);
        } else {
          this.title = `Edit ${cls.singularName}`;
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.dialogRef.close(this.object);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
