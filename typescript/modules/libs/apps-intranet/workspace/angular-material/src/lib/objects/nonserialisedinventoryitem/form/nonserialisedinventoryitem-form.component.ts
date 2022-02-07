import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import { NonSerialisedInventoryItem } from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './nonserialisedinventoryitem-edit.component.html',
  providers: [ContextService],
})
export class NonSerialisedInventoryItemEditComponent
  implements OnInit, OnDestroy
{
  public m: M;
  public title: string;

  public nonSerialisedInventoryItem: NonSerialisedInventoryItem;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<NonSerialisedInventoryItemEditComponent>,

    public refreshService: RefreshService,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;
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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.nonSerialisedInventoryItem);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
