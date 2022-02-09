import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  OrderAdjustment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './orderadjustment-form.component.html',
  providers: [ContextService],
})
export class OrderAdjustmentFormComponent
  extends AllorsFormComponent<OrderAdjustment>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public m: M;

  public title: string;

  public container: IObject;
  public object: OrderAdjustment;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([this.refreshService.refresh$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const cls = this.data.strategy?.cls;
          const { associationRoleType } = this.data;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.OrderAdjustment({
                objectId: this.data.id,
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Quote({ objectId: this.data.associationId }),
              pull.Order({ objectId: this.data.associationId }),
              pull.Invoice({ objectId: this.data.associationId })
            );
          }

          return this.allors.context.pull(pulls).pipe(
            map((loaded) => ({
              loaded,
              create: isCreate,
              cls,
              associationRoleType,
            }))
          );
        })
      )
      .subscribe(({ loaded, create, cls, associationRoleType }) => {
        this.allors.context.reset();

        this.container =
          loaded.object<Quote>(m.Quote) ??
          loaded.object<Order>(m.Order) ??
          loaded.object<Invoice>(m.Invoice);
        this.object = loaded.object<OrderAdjustment>(m.OrderAdjustment);

        if (create) {
          this.title = `Add ${cls.singularName}`;
          this.object = this.allors.context.create<OrderAdjustment>(cls);
          this.container.strategy.addCompositesRole(
            associationRoleType,
            this.object
          );
        } else {
          this.title = `Edit ${this.object.strategy.cls.singularName}`;
        }
      });
  }
}
