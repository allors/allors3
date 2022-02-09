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
  SupplierOffering,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './supplieroffering-form.component.html',
  providers: [ContextService],
})
export class SupplierOfferingFormComponent
  extends AllorsFormComponent<SupplierOffering>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  supplierOffering: SupplierOffering;
  part: Part;
  ratingTypes: RatingType[];
  preferences: Ordinal[];
  unitsOfMeasure: UnitOfMeasure[];
  currencies: Currency[];
  settings: Settings;

  private subscription: Subscription;
  title: string;

  allSuppliersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
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

          let pulls = [
            this.fetcher.Settings,
            pull.RatingType({ sorting: [{ roleType: m.RateType.Name }] }),
            pull.Ordinal({ sorting: [{ roleType: m.Ordinal.Name }] }),
            pull.UnitOfMeasure({
              sorting: [{ roleType: m.UnitOfMeasure.Name }],
            }),
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
          ];

          if (isCreate) {
            pulls = [
              ...pulls,
              pull.Part({
                objectId: this.data.associationId,
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

          return this.allors.context
            .pull(pulls)
            .pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        this.ratingTypes = loaded.collection<RatingType>(m.RatingType);
        this.preferences = loaded.collection<Ordinal>(m.Ordinal);
        this.unitsOfMeasure = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.settings = this.fetcher.getSettings(loaded);

        if (isCreate) {
          this.title = 'Add supplier offering';

          this.supplierOffering = this.allors.context.create<SupplierOffering>(
            m.SupplierOffering
          );
          this.part = loaded.object<Part>(m.Part);
          this.supplierOffering.Part = this.part;
          this.supplierOffering.Currency = this.settings.PreferredCurrency;
        } else {
          this.supplierOffering = loaded.object<SupplierOffering>(
            m.SupplierOffering
          );
          this.part = this.supplierOffering.Part;

          if (this.supplierOffering.canWritePrice) {
            this.title = 'Edit supplier offering';
          } else {
            this.title = 'View supplier offering';
          }
        }
      });
  }
}