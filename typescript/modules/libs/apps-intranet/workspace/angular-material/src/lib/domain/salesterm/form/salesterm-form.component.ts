import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  SalesInvoice,
  SalesOrder,
  SalesTerm,
  TermType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { AssociationType, RoleType } from '@allors/system/workspace/meta';

@Component({
  templateUrl: './salesterm-form.component.html',
  providers: [ContextService],
})
export class SalesTermFormComponent extends AllorsFormComponent<SalesTerm> {
  public m: M;

  public container: IObject;
  public termTypes: TermType[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.TermType({
        predicate: {
          kind: 'Equals',
          propertyType: m.TermType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.TermType.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.SalesTerm({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            TermType: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.SalesInvoice({ objectId: initializer.id }),
        p.SalesOrder({ objectId: initializer.id })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.container =
      pullResult.object<SalesInvoice>(this.m.SalesInvoice) ||
      pullResult.object<SalesOrder>(this.m.SalesOrder);

    this.termTypes = pullResult.collection<TermType>(this.m.TermType);

    // TODO: KOEN
    // this.termTypes = this.termTypes?.filter(
    //   (v) => v.strategy.cls.singularName === `${cls.singularName}Type`
    // );

    if (this.createRequest) {
      const associationType = this.createRequest?.initializer.propertyType as AssociationType;
      const roleType = associationType.roleType; 
      this.container.strategy.addCompositesRole(
        roleType,
        this.object
      );
    }
  }
}
