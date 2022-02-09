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
  Carrier,
  InternalOrganisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './carrier-form.component.html',
  providers: [ContextService],
})
export class CarrierFormComponent
  extends AllorsFormComponent<Carrier>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public title: string;
  public subTitle: string;

  public m: M;

  public carrier: Carrier;

  public characteristics: SerialisedItemCharacteristicType[];

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

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.Carrier({
                objectId: this.data.id,
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
          this.title = 'Add Carrier';
          this.carrier = this.allors.context.create<Carrier>(m.Carrier);
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
}
