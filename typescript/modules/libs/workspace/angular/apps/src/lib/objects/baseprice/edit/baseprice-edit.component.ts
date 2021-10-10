import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Good, InternalOrganisation, NonUnifiedGood, Part, PriceComponent } from '@allors/workspace/domain/default';
import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './baseprice-edit.component.html',
  providers: [ContextService],
})
export class BasepriceEditComponent extends TestScope implements OnInit, OnDestroy {
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
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

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

          return this.allors.context.pull(pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation = loaded.object<InternalOrganisation>(this.m.InternalOrganisation);
        this.nonUnifiedGood = loaded.object<NonUnifiedGood>(this.m.NonUnifiedGood);
        this.part = loaded.object<Part>(this.m.Part);

        if (isCreate) {
          this.title = 'Add base price';

          this.priceComponent = this.allors.context.create<PriceComponent>(this.m.BasePrice);
          this.priceComponent.FromDate = new Date();
          this.priceComponent.PricedBy = this.internalOrganisation;

          if (this.nonUnifiedGood) {
            this.priceComponent.Product = this.nonUnifiedGood;
          }

          if (this.part) {
            this.priceComponent.Part = this.part;
          }
        } else {
          this.priceComponent = loaded.object<PriceComponent>(this.m.PriceComponent);

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
    }, this.saveService.errorHandler);
  }
}
