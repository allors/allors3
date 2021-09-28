import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { ObjectData, RefreshService, SaveService, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { Good, InternalOrganisation, M, Part, PriceComponent } from '@allors/workspace/meta/default';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './baseprice-edit.component.html',
  providers: [SessionService],
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
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<BasepriceEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pullBuilder: pull } = this.m; const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          let pulls = [this.fetcher.internalOrganisation];

          if (!isCreate) {
            pulls = [
              ...pulls,
              pull.PriceComponent({
                object: this.data.id,
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
                object: this.data.associationId,
              }),
              pull.Part({
                object: this.data.associationId,
              }),
            ];
          }

          return this.allors.context.load(new PullRequest({ pulls })).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.internalOrganisation = loaded.objects.InternalOrganisation as Organisation;
        this.nonUnifiedGood = loaded.objects.NonUnifiedGood as NonUnifiedGood;
        this.part = loaded.objects.Part as Part;

        if (isCreate) {
          this.title = 'Add base price';

          this.priceComponent = this.allors.context.create('BasePrice') as PriceComponent;
          this.priceComponent.FromDate = new Date().toISOString();
          this.priceComponent.PricedBy = this.internalOrganisation;

          if (this.nonUnifiedGood) {
            this.priceComponent.Product = this.nonUnifiedGood;
          }

          if (this.part) {
            this.priceComponent.Part = this.part;
          }
        } else {
          this.priceComponent = loaded.objects.PriceComponent as PriceComponent;

          if (this.priceComponent.CanWritePrice) {
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
    this.allors.context.save().subscribe(() => {
      const data: IObject = {
        id: this.priceComponent.id,
        objectType: this.priceComponent.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
