import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, map } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import {
  Good,
  InternalOrganisation,
  NonUnifiedGood,
  Part,
  PriceComponent,
  Brand,
  Model,
  Locale,
  Carrier,
  SerialisedItemCharacteristicType,
  WorkTask,
  ContactMechanism,
  Person,
  Organisation,
  PartyContactMechanism,
  OrganisationContactRelationship,
  Catalogue,
  Singleton,
  ProductCategory,
  Scope,
} from '@allors/workspace/domain/default';
import {
  Action,
  DeleteService,
  EditService,
  Filter,
  FilterDefinition,
  MediaService,
  NavigationService,
  ObjectData,
  OverviewService,
  RefreshService,
  SaveService,
  SearchFactory,
  Sorter,
  Table,
  TableRow,
  TestScope,
} from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';
import { And } from '@allors/workspace/domain/system';

import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './catalogue-edit.component.html',
  providers: [SessionService],
})
export class CatalogueEditComponent extends TestScope implements OnInit, OnDestroy {
  public m: M;

  public catalogue: Catalogue;
  public title: string;

  public subTitle: string;

  public singleton: Singleton;
  public locales: Locale[];
  public categories: ProductCategory[];
  public scopes: Scope[];
  public internalOrganisation: InternalOrganisation;

  private subscription: Subscription;

  constructor(
    @Self() public allors: SessionService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public dialogRef: MatDialogRef<CatalogueEditComponent>,

    private refreshService: RefreshService,
    private saveService: SaveService,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([this.refreshService.refresh$, this.internalOrganisationId.observable$])
      .pipe(
        switchMap(() => {
          const isCreate = this.data.id === undefined;

          const pulls = [this.fetcher.categories, this.fetcher.locales, this.fetcher.internalOrganisation, pull.Scope({})];

          if (!isCreate) {
            pulls.push(
              pull.Catalogue({
                objectId: this.data.id,
                include: {
                  CatalogueImage: x,
                  LocalisedNames: {
                    Locale: x,
                  },
                  LocalisedDescriptions: {
                    Locale: x,
                  },
                },
              })
            );
          }

          return this.allors.client.pullReactive(this.allors.session, pulls).pipe(map((loaded) => ({ loaded, create: isCreate })));
        })
      )
      .subscribe(({ loaded, create }) => {
        this.allors.session.reset();

        this.catalogue = loaded.object<Catalogue>(m.Catalogue);
        this.locales = loaded.collection<Locale>(m.Locale);
        this.categories = loaded.collection<ProductCategory>(m.ProductCategory);
        this.scopes = loaded.collection<Scope>(m.Scope);
        this.internalOrganisation = loaded.object<InternalOrganisation>(m.InternalOrganisation);

        if (create) {
          this.title = 'Add Catalogue';
          this.catalogue = this.allors.session.create<Catalogue>(m.Catalogue);
          this.catalogue.InternalOrganisation = this.internalOrganisation;
        } else {
          if (this.catalogue.canWriteCatScope) {
            this.title = 'Edit Catalogue';
          } else {
            this.title = 'View Catalogue';
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
        id: this.catalogue.id,
        objectType: this.catalogue.objectType,
      };

      this.dialogRef.close(data);
      this.refreshService.refresh();
    }, this.saveService.errorHandler);
  }
}
