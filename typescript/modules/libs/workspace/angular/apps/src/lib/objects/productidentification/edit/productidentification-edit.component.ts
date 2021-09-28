import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService, Saved } from '@allors/angular/services/core';
import { ProductIdentification, ProductIdentificationType } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { IObject, IObject } from '@allors/domain/system';
import { Equals, Sort } from '@allors/data/system';
import { TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './productidentification-edit.component.html',
  providers: [SessionService]
})
export class ProductIdentificationEditComponent extends TestScope implements OnInit, OnDestroy {

  public m: M;

  public title = 'Edit IGood Identification';

  public container: IObject;
  public object: ProductIdentification;
  public productIdentificationTypes: ProductIdentificationType[];

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<ProductIdentificationEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
  ) {

    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {

    const m = this.allors.workspace.configuration.metaPopulation as M; const { pullBuilder: pull } = m; const x = {};

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {

          const isCreate = (this.data as IObject).id === undefined;
          const { objectType, associationRoleType } = this.data;

          const pulls = [
            pull.ProductIdentificationType({
              predicate: { kind: 'Equals', propertyType: m.ProductIdentificationType.IsActive, value: true },
              sort: [
                new Sort(m.ProductIdentificationType.Name),
              ],
            })
          ];

          if (!isCreate) {
            pulls.push(
              pull.ProductIdentification(
                {
                  objectId: this.data.id,
                  include: {
                    ProductIdentificationType: x,
                  }
                }),
            );
          }

          if (isCreate && this.data.associationId) {
            pulls.push(
              pull.Good({ object: this.data.associationId }),
              pull.Part({ object: this.data.associationId }),
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls)
            .pipe(
              map((loaded) => ({ loaded, create: isCreate, objectType, associationRoleType }))
            );
        })
      )
      .subscribe(({ loaded, create, objectType, associationRoleType }) => {
        this.allors.session.reset();

        this.container = loaded.objects.Good || loaded.objects.Part;
        this.object = loaded.object<ProductIdentification>(m.ProductIdentification);
        this.productIdentificationTypes = loaded.collection<ProductIdentificationType>(m.ProductIdentificationType);

        if (create) {
          this.title = 'Add Identification';
          this.object = this.allors.context.create(objectType) as ProductIdentification;
          this.container.add(associationRoleType, this.object);
        }

      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.allors.client.pushReactive(this.allors.session)
      .subscribe(() => {
        const data: IObject = {
          id: this.object.id,
          objectType: this.object.objectType,
        };

        this.dialogRef.close(data);
        this.refreshService.refresh();
      },
        this.saveService.errorHandler
      );
  }
}
