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
  ProductIdentification,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './productidentification-form.component.html',
  providers: [ContextService],
})
export class ProductIdentificationFormComponent
  extends AllorsFormComponent<ProductIdentification>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public m: M;

  public title = 'Edit Good Identification';

  public container: IObject;
  public object: ProductIdentification;
  public productIdentificationTypes: ProductIdentificationType[];

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
            pull.ProductIdentificationType({
              predicate: {
                kind: 'Equals',
                propertyType: m.ProductIdentificationType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.ProductIdentificationType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductIdentification({
                objectId: this.data.id,
                include: {
                  ProductIdentificationType: x,
                },
              })
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Good({ objectId: this.data.associationId }),
              pull.Part({ objectId: this.data.associationId })
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
          loaded.object<Good>(m.Good) || loaded.object<Part>(m.Part);
        this.object = loaded.object<ProductIdentification>(
          m.ProductIdentification
        );
        this.productIdentificationTypes =
          loaded.collection<ProductIdentificationType>(
            m.ProductIdentificationType
          );

        if (create) {
          this.title = 'Add Identification';
          this.object = this.allors.context.create<ProductIdentification>(cls);
          this.container.strategy.addCompositesRole(
            associationRoleType,
            this.object
          );
        }
      });
  }
}
