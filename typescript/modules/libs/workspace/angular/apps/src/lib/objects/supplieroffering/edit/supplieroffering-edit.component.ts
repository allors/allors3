import { Component, Self, OnInit, OnDestroy, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';
import { isBefore, isAfter } from 'date-fns';

import { MetaService, SessionService, RefreshService } from '@allors/angular/services/core';
import {
  Organisation,
  SupplierOffering,
  Part,
  RatingType,
  Ordinal,
  UnitOfMeasure,
  Currency,
  Settings,
  SupplierRelationship,
} from '@allors/domain/generated';
import { Meta } from '@allors/meta/generated';
import { ObjectData, SaveService } from '@allors/angular/material/services/core';
import { Filters, FetcherService, InternalOrganisationId } from '@allors/angular/base';
import { Sort } from '@allors/data/system';
import { PullRequest } from '@allors/protocol/system';
import { IObject } from '@allors/domain/system';
import { TestScope, SearchFactory } from '@allors/angular/core';

@Component({
  templateUrl: './supplieroffering-edit.component.html',
  providers: [SessionService],
})
export class SupplierOfferingEditComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  supplierOffering: SupplierOffering;
  part: Part;
  ratingTypes: RatingType[];
  preferences: Ordinal[];
  activeSuppliers: Organisation[];
  unitsOfMeasure: UnitOfMeasure[];
  currencies: Currency[];
  settings: Settings;

  private subscription: Subscription;
  title: string;

  allSuppliersFilter: SearchFactory;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SupplierOfferingEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { pull, x, m } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          let pulls = [
            this.fetcher.Settings,
            pull.RatingType({ sorting: [{ roleType: m.RateType.Name }] }),
            pull.Ordinal({ sorting: [{ roleType: m.Ordinal.Name }] }),
            pull.UnitOfMeasure({ sorting: [{ roleType: m.UnitOfMeasure.Name }] }),
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
          ];

          if (isCreate) {
            pulls = [
              ...pulls,
              pull.Part({
                object: this.data.associationId,
                include: {
                  SuppliedBy: x,
                },
              }),
            ];
          }

          if (!isCreate) {
            pulls = [
              ...pulls,
              pull.SupplierOffering({
                objectId: this.data.id,
                include: {
                  Part: x,
                  Rating: x,
                  Preference: x,
                  Supplier: x,
                  Currency: x,
                  UnitOfMeasure: x,
                },
              }),
            ];
          }

          this.allSuppliersFilter = Filters.allSuppliersFilter(m);

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.ratingTypes = loaded.collection<RatingType>(m.RatingType);
        this.preferences = loaded.collection<Ordinal>(m.Ordinal);
        this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.settings = loaded.object<Settings>(m.Settings);

        if (isCreate) {
          this.title = 'Add supplier offering';

          this.supplierOffering = this.allors.session.create<SupplierOffering>(m.SupplierOffering);
          this.part = loaded.object<Part>(m.Part);
          this.supplierOffering.Part = this.part;
          this.supplierOffering.Currency = this.settings.PreferredCurrency;
        } else {
          this.supplierOffering = loaded.object<SupplierOffering>(m.SupplierOffering);
          this.part = this.supplierOffering.Part;

          if (this.supplierOffering.canWritePrice) {
            this.title = 'Edit supplier offering';
          } else {
            this.title = 'View supplier offering';
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
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.supplierOffering.id,
        objectType: this.supplierOffering.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
