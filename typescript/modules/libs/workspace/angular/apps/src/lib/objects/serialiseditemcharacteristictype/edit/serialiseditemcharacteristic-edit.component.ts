import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { SessionService, MetaService, RefreshService } from '@allors/angular/services/core';
import { TimeFrequency, SerialisedItemCharacteristicType, UnitOfMeasure, Singleton, IUnitOfMeasure, Locale } from '@allors/domain/generated';
import { PullRequest } from '@allors/protocol/system';
import { Meta } from '@allors/meta/generated';
import { SaveService, ObjectData } from '@allors/angular/material/services/core';
import { InternalOrganisationId, FetcherService } from '@allors/angular/base';
import { IObject } from '@allors/domain/system';
import { Equals, Sort } from '@allors/data/system';
import { TestScope } from '@allors/angular/core';

@Component({
  templateUrl: './serialiseditemcharacteristic-edit.component.html',
  providers: [SessionService],
})
export class SerialisedItemCharacteristicEditComponent extends TestScope implements OnInit, OnDestroy {
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
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<SerialisedItemCharacteristicEditComponent>,
    
    public refreshService: RefreshService,
    private saveService: SaveService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$, this.internalOrganisationId.observable$)
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

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
              predicate: { kind: 'Equals', propertyType: m.UnitOfMeasure.IsActive, value: true },
              sorting: [{ roleType: m.UnitOfMeasure.Name }],
            }),
            pull.TimeFrequency({
              predicate: { kind: 'Equals', propertyType: m.TimeFrequency.IsActive, value: true },
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
              }),
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, isCreate })));
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.session.reset();

        this.singleton = loaded.collections.Singletons[0] as Singleton;
        this.uoms = loaded.collection<UnitOfMeasure>(m.UnitOfMeasure);
        this.timeFrequencies = loaded.collection<TimeFrequency>(m.TimeFrequency);
        this.allUoms = this.uoms.concat(this.timeFrequencies).sort((a, b) => (a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0));
        this.locales = loaded.collection<Locale>(m.Locale);

        if (isCreate) {
          this.title = 'Add Product Characteristic';

          this.productCharacteristic = this.allors.session.create<SerialisedItemCharacteristicType>(m.SerialisedItemCharacteristicType);
          this.productCharacteristic.IsActive = true;
        } else {
          this.productCharacteristic = loaded.object<SerialisedItemCharacteristicType>(m.SerialisedItemCharacteristicType);

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
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      const data: IObject = {
        id: this.productCharacteristic.id,
        objectType: this.productCharacteristic.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
