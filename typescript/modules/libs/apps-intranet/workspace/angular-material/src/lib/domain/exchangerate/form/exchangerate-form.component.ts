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
  ExchangeRate,
  InternalOrganisation,
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
  templateUrl: './exchangerate-form.component.html',
  providers: [ContextService],
})
export class ExchangeRateFormComponent
  extends AllorsFormComponent<ExchangeRate>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public title: string;
  public subTitle: string;

  public m: M;

  public exchangeRate: ExchangeRate;
  internalOrganisation: InternalOrganisation;
  currencies: Currency[];

  private subscription: Subscription;

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

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const isCreate = this.data.id == null;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.ExchangeRate({
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
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.currencies = loaded.collection<Currency>(m.Currency);

        if (isCreate) {
          this.title = 'Add Position Type';
          this.exchangeRate = this.allors.context.create<ExchangeRate>(
            m.ExchangeRate
          );
          this.exchangeRate.ToCurrency =
            this.internalOrganisation.PreferredCurrency;
        } else {
          this.exchangeRate = loaded.object<ExchangeRate>(m.ExchangeRate);

          if (this.exchangeRate.canWriteRate) {
            this.title = 'Edit Exchange Rate';
          } else {
            this.title = 'View Exchange Rate';
          }
        }
      });
  }
}
