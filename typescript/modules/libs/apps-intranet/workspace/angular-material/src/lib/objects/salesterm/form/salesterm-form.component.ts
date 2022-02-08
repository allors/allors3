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
  SalesTerm,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './salesterm-form.component.html',
  providers: [ContextService],
})
export class SalesTermFormComponent
  extends AllorsFormComponent<SalesTerm>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public m: M;

  public title = 'Edit Term Type';

  public container: IObject;
  public object: SalesTerm;
  public termTypes: TermType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;
          const cls = this.data.strategy?.cls;
          const { associationRoleType } = this.data;

          const pulls = [
            pull.TermType({
              predicate: {
                kind: 'Equals',
                propertyType: m.TermType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.TermType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.SalesTerm({
                objectId: this.data.id,
                include: {
                  TermType: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.SalesInvoice({ objectId: this.data.associationId }),
              pull.SalesOrder({ objectId: this.data.associationId })
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
          loaded.object<SalesInvoice>(m.SalesInvoice) ||
          loaded.object<SalesOrder>(m.SalesOrder);
        this.object = loaded.object<SalesTerm>(m.SalesTerm);
        this.termTypes = loaded.collection<TermType>(m.TermType);
        this.termTypes = this.termTypes?.filter(
          (v) => v.strategy.cls.singularName === `${cls.singularName}Type`
        );

        if (create) {
          this.title = 'Add Sales Term';
          this.object = this.allors.context.create<SalesTerm>(cls);
          this.container.strategy.addCompositesRole(
            associationRoleType,
            this.object
          );
        }
      });
  }
}
