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
  NonSerialisedInventoryItem,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './nonserialisedinventoryitem-form.component.html',
  providers: [ContextService],
})
export class NonSerialisedInventoryItemFormComponent
  extends AllorsFormComponent<NonSerialisedInventoryItem>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public m: M;
  public title: string;

  public nonSerialisedInventoryItem: NonSerialisedInventoryItem;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          const pulls = [];

          if (!isCreate) {
            pulls.push(
              pull.NonSerialisedInventoryItem({
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

        this.nonSerialisedInventoryItem =
          loaded.object<NonSerialisedInventoryItem>(
            m.NonSerialisedInventoryItem
          );

        if (this.nonSerialisedInventoryItem.canWritePartLocation) {
          this.title = 'Edit Inventory Item';
        } else {
          this.title = 'View Inventory Item';
        }
      });
  }
}
