import { Injectable } from '@angular/core';
import { SingletonId } from '@allors/workspace/angular/base';
import { WorkspaceService } from '@allors/workspace/angular/core';
import { M, PullBuilder } from '@allors/workspace/meta/default';
import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { Facility, Locale, InternalOrganisation, ProductCategory, Settings } from '@allors/workspace/domain/default';
import { InternalOrganisationId } from '../state/internal-organisation-id';

const x = {};

@Injectable({
  providedIn: 'root',
})
export class FetcherService {
  m: M;
  pull: PullBuilder;

  constructor(private singletonId: SingletonId, private internalOrganisationId: InternalOrganisationId, private workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    this.pull = this.m.pullBuilder;
  }

  public get internalOrganisation(): Pull {
    return this.pull.InternalOrganisation({
      name: 'FetcherInternalOrganisation',
      objectId: this.internalOrganisationId.value,
      include: {
        DefaultPaymentMethod: x,
        DefaultShipmentMethod: x,
        DefaultCollectionMethod: x,
        PaymentMethods: x,
        Organisation_AssignedActiveCollectionMethods: x,
        Organisation_DerivedActiveCollectionMethods: x,
        PreferredCurrency: x,
        Locale: x,
        OrderAddress: x,
        BillingAddress: x,
        ShippingAddress: x,
        GeneralCorrespondence: x,
        Country: {
          DerivedVatRegimes: {
            VatRates: x,
          },
        },
      },
    });
  }

  getInternalOrganisation(loaded: IPullResult) {
    return loaded.object<InternalOrganisation>('FetcherInternalOrganisation');
  }

  public get warehouses(): Pull {
    return this.pull.Facility({
      name: 'FetcherWarehouses',
      predicate: {
        kind: 'ContainedIn',
        propertyType: this.m.Facility.FacilityType,
        extent: {
          kind: 'Filter',
          objectType: this.m.FacilityType,
          predicate: {
            kind: 'Equals',
            propertyType: this.m.FacilityType.UniqueId,
            value: 'd4a70252-58d0-425b-8f54-7f55ae01a7b3',
          },
        },
      },
      include: {
        Owner: x,
      },
      sorting: [{ roleType: this.m.Facility.Name }],
    });
  }

  getWarehouses(loaded: IPullResult) {
    return loaded.collection<Facility>('FetcherWarehouses');
  }

  public get ownWarehouses(): Pull {
    return this.pull.Facility({
      name: 'FetcherOwnWarehouses',
      predicate: {
        kind: 'And',
        operands: [
          {
            kind: 'Equals',
            propertyType: this.m.Facility.Owner,
            value: this.internalOrganisationId.value,
          },
          {
            kind: 'ContainedIn',
            propertyType: this.m.Facility.FacilityType,
            extent: {
              kind: 'Filter',
              objectType: this.m.FacilityType,
              predicate: {
                kind: 'Equals',
                propertyType: this.m.FacilityType.UniqueId,
                value: 'd4a70252-58d0-425b-8f54-7f55ae01a7b3',
              },
            },
          },
        ],
      },
      sorting: [{ roleType: this.m.Facility.Name }],
    });
  }

  getOwnWarehouses(loaded: IPullResult) {
    return loaded.collection<Facility>('FetcherOwnWarehouses');
  }

  public get categories(): Pull {
    return this.pull.Organisation({
      name: 'FetcherProductCategories',
      objectId: this.internalOrganisationId.value,
      select: { ProductCategoriesWhereInternalOrganisation: x },
    });
  }

  getProductCategories(loaded: IPullResult) {
    return loaded.collection<ProductCategory>('FetcherProductCategories');
  }

  public get locales(): Pull {
    return this.pull.Singleton({
      name: 'FetcherAdditionalLocales',
      objectId: this.singletonId.value,
      select: {
        AdditionalLocales: {
          include: {
            Language: x,
            Country: x,
          },
        },
      },
    });
  }

  getAdditionalLocales(loaded: IPullResult) {
    return loaded.collection<Locale>('FetcherAdditionalLocales');
  }

  public get Settings(): Pull {
    return this.pull.Singleton({
      name: 'FetcherSettings',
      objectId: this.singletonId.value,
      select: {
        Settings: {
          include: {
            PreferredCurrency: x,
            DefaultFacility: x,
          },
        },
      },
    });
  }

  getSettings(loaded: IPullResult) {
    return loaded.object<Settings>('FetcherSettings');
  }
}
