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
  SerialisedItemCharacteristicType,
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
  templateUrl: './serialiseditemcharacteristic-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemCharacteristicFormComponent
  extends AllorsFormComponent<SerialisedItemCharacteristicType>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  public title: string;
  public subTitle: string;

  public m: M;

  public productCharacteristic: SerialisedItemCharacteristicType;

  public singleton: Singleton;
  public uoms: IUnitOfMeasure[];
  public timeFrequencies: TimeFrequency[];
  public allUoms: IUnitOfMeasure[];

  private subscription: Subscription;
  locales: Locale[];

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

          const pulls = [
            this.fetcher.locales,
            pull.Singleton({
              include: {
                AdditionalLocales: {
                  Language: x,
                },
              },
            }),
            pull.UnitOfMeasure({
              predicate: {
                kind: 'Equals',
                propertyType: m.UnitOfMeasure.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.UnitOfMeasure.Name }],
            }),
            pull.TimeFrequency({
              predicate: {
                kind: 'Equals',
                propertyType: m.TimeFrequency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.TimeFrequency.Name }],
            }),
          ];

          if (!isCreate) {
            pulls.push(
              pull.SerialisedItemCharacteristicType({
                objectId: this.data.id,
                include: {
                  LocalisedNames: {
                    Locale: x,
                  },
                },
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

        this.singleton = loaded.collection<Singleton>(m.Singleton)[0];
        this.uoms = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.timeFrequencies = loaded.collection<TimeFrequency>(
          m.TimeFrequency
        );
        this.allUoms = this.uoms
          .concat(this.timeFrequencies)
          .sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0));
        this.locales = this.fetcher.getAdditionalLocales(loaded);

        if (isCreate) {
          this.title = 'Add Product Characteristic';

          this.productCharacteristic =
            this.allors.context.create<SerialisedItemCharacteristicType>(
              m.SerialisedItemCharacteristicType
            );
          this.productCharacteristic.IsActive = true;
        } else {
          this.productCharacteristic =
            loaded.object<SerialisedItemCharacteristicType>(
              m.SerialisedItemCharacteristicType
            );

          if (this.productCharacteristic.canWriteName) {
            this.title = 'Edit Product Characteristic';
          } else {
            this.title = 'View Product Characteristic';
          }
        }
      });
  }
}
