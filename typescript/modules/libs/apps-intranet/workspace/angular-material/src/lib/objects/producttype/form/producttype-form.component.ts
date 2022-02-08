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
  ProductType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './producttype-form.component.html',
  providers: [ContextService],
})
export class ProductTypeFormComponent
  extends AllorsFormComponent<ProductType>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public title: string;
  public subTitle: string;

  public m: M;

  public productType: ProductType;

  public characteristics: SerialisedItemCharacteristicType[];

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

          const pulls = [
            pull.SerialisedItemCharacteristicType({
              sorting: [{ roleType: m.SerialisedItemCharacteristicType.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductType({
                objectId: this.data.id,
                include: {
                  SerialisedItemCharacteristicTypes: x,
                },
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
          this.title = 'Add Product Type';
          this.productType = this.allors.context.create<ProductType>(
            m.ProductType
          );
        } else {
          this.productType = loaded.object<ProductType>(m.ProductType);

          if (this.productType.canWriteName) {
            this.title = 'Edit Product Type';
          } else {
            this.title = 'View Product Type';
          }
        }
      });
  }
}
