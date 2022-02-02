import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { M } from '@allors/default/workspace/meta';
import {
  Good,
  InternalOrganisation,
  NonUnifiedGood,
  Part,
  PriceComponent,
} from '@allors/default/workspace/domain';
import {
  ObjectData,
  RefreshService,
  ErrorService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './baseprice-form.component.html',
  providers: [ContextService],
})
export class BasepriceEditComponent implements OnInit, OnDestroy {
  readonly m: M;

  nonUnifiedGood: Good;
  part: Part;
  priceComponent: PriceComponent;
  internalOrganisation: InternalOrganisation;
  item: Good | Part;
  title: string;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<BasepriceEditComponent>,
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

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id == null;

          let pulls = [this.fetcher.internalOrganisation];

          if (!isCreate) {
            pulls = [
              ...pulls,
              pull.PriceComponent({
                objectId: this.data.id,
                include: {
                  Currency: x,
                },
              }),
            ];
          }

          if (isCreate && this.data.associationId) {
            pulls = [
              ...pulls,
              pull.NonUnifiedGood({
                objectId: this.data.associationId,
              }),
              pull.Part({
                objectId: this.data.associationId,
              }),
            ];
          }

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.nonUnifiedGood = loaded.object<NonUnifiedGood>(
          this.m.NonUnifiedGood
        );
        this.part = loaded.object<Part>(this.m.Part);

        if (isCreate) {
          this.title = 'Add base price';

          this.priceComponent = this.allors.context.create<PriceComponent>(
            this.m.BasePrice
          );
          this.priceComponent.FromDate = new Date();
          this.priceComponent.PricedBy = this.internalOrganisation;

          if (this.nonUnifiedGood) {
            this.priceComponent.Product = this.nonUnifiedGood;
          }

          if (this.part) {
            this.priceComponent.Part = this.part;
          }
        } else {
          this.priceComponent = loaded.object<PriceComponent>(
            this.m.PriceComponent
          );

          if (this.priceComponent.canWritePrice) {
            this.title = 'Edit base price';
          } else {
            this.title = 'View base price';
          }
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
      this.dialogRef.close(this.priceComponent);
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
