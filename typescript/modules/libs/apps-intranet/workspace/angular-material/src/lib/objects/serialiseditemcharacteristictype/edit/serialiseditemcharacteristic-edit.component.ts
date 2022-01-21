import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Locale,
  UnitOfMeasure,
  TimeFrequency,
  Singleton,
  SerialisedItemCharacteristicType,
  IUnitOfMeasure,
} from '@allors/workspace/domain/default';
import {
  ObjectData,
  RefreshService,
  SaveService,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { IObject } from '@allors/system/workspace/domain';

import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './serialiseditemcharacteristic-edit.component.html',
  providers: [ContextService],
})
export class SerialisedItemCharacteristicEditComponent
  implements OnInit, OnDestroy
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
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SerialisedItemCharacteristicEditComponent>,
    public refreshService: RefreshService,
    private saveService: SaveService,
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

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.dialogRef.close(this.productCharacteristic);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
