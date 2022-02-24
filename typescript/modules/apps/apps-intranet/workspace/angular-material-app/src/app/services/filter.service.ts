import { Injectable } from '@angular/core';
import {
  Filter,
  FilterDefinition,
  FilterService,
  SearchFactory,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import {
  Brand,
  Country,
  Currency,
  Facility,
  FixedAsset,
  Good,
  InventoryItemKind,
  IUnitOfMeasure,
  Model,
  Organisation,
  Ownership,
  Part,
  PartCategory,
  Party,
  Person,
  PositionType,
  Priority,
  Product,
  ProductCategory,
  ProductIdentification,
  ProductType,
  PurchaseInvoiceState,
  PurchaseInvoiceType,
  PurchaseOrderState,
  QuoteState,
  RateType,
  RequestState,
  RequirementState,
  SalesInvoiceState,
  SalesInvoiceType,
  SalesOrderState,
  Scope,
  SerialisedItem,
  SerialisedItemAvailability,
  SerialisedItemState,
  ShipmentState,
  WorkEffortState,
} from '@allors/default/workspace/domain';
import {
  Filters,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';

@Injectable()
export class AppFilterService implements FilterService {
  filterByComposite: Map<Composite, Filter>;
  filterDefinitionByComposite: Map<Composite, FilterDefinition>;

  constructor(
    workspaceService: WorkspaceService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    this.filterDefinitionByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    const define = (
      composite: Composite,
      filterDefinition: FilterDefinition
    ) => {
      this.filterDefinitionByComposite.set(composite, filterDefinition);
    };

    const currencySearch = new SearchFactory({
      objectType: m.Currency,
      roleTypes: [m.Currency.IsoCode],
    });

    const requirementStateSearch = new SearchFactory({
      objectType: m.RequirementState,
      roleTypes: [m.RequirementState.Name],
    });

    const prioritySearch = new SearchFactory({
      objectType: m.Priority,
      predicates: [
        { kind: 'Equals', propertyType: m.Enumeration.IsActive, value: true },
      ],
      roleTypes: [m.Priority.Name],
    });

    const inventoryItemKindSearch = new SearchFactory({
      objectType: m.InventoryItemKind,
      predicates: [
        { kind: 'Equals', propertyType: m.Enumeration.IsActive, value: true },
      ],
      roleTypes: [m.InventoryItemKind.Name],
    });

    const manufacturerSearch = new SearchFactory({
      objectType: m.Organisation,
      predicates: [
        {
          kind: 'Equals',
          propertyType: m.Organisation.IsManufacturer,
          value: true,
        },
      ],
      roleTypes: [m.Organisation.DisplayName],
    });

    const facilitySearch = new SearchFactory({
      objectType: m.Facility,
      roleTypes: [m.Facility.Name],
    });

    const countrySearch = new SearchFactory({
      objectType: m.Country,
      roleTypes: [m.Country.Name],
    });

    const modelSearch = new SearchFactory({
      objectType: m.Model,
      roleTypes: [m.Model.Name],
    });

    const brandSearch = new SearchFactory({
      objectType: m.Brand,
      roleTypes: [m.Brand.Name],
    });

    const categorySearch = new SearchFactory({
      objectType: m.ProductCategory,
      roleTypes: [m.ProductCategory.DisplayName],
    });

    const scopeSearch = new SearchFactory({
      objectType: m.Scope,
      roleTypes: [m.Scope.Name],
    });

    const productSearch = new SearchFactory({
      objectType: m.Good,
      roleTypes: [m.Good.Name],
    });

    const partSearch = new SearchFactory({
      objectType: m.NonUnifiedPart,
      roleTypes: [m.NonUnifiedPart.Name, m.NonUnifiedPart.SearchString],
    });

    const productTypeSearch = new SearchFactory({
      objectType: m.ProductType,
      roleTypes: [m.ProductType.Name],
    });

    const productIdSearch = new SearchFactory({
      objectType: m.ProductIdentification,
      roleTypes: [m.ProductIdentification.Identification],
    });

    const uomSearch = new SearchFactory({
      objectType: m.IUnitOfMeasure,
      roleTypes: [m.IUnitOfMeasure.Name],
      predicates: [
        {
          kind: 'Equals',
          propertyType: m.IUnitOfMeasure.IsActive,
          value: true,
        },
      ],
    });

    const serialisedItemStateSearch = new SearchFactory({
      objectType: m.SerialisedItemState,
      roleTypes: [m.SerialisedItemState.Name],
      predicates: [
        {
          kind: 'Equals',
          propertyType: m.SerialisedItemState.IsActive,
          value: true,
        },
      ],
    });

    const serialisedItemAvailabilitySearch = new SearchFactory({
      objectType: m.SerialisedItemAvailability,
      roleTypes: [m.SerialisedItemAvailability.Name],
      predicates: [
        {
          kind: 'Equals',
          propertyType: m.SerialisedItemAvailability.IsActive,
          value: true,
        },
      ],
    });

    const ownershipSearch = new SearchFactory({
      objectType: m.Ownership,
      roleTypes: [m.Ownership.Name],
      predicates: [
        { kind: 'Equals', propertyType: m.Ownership.IsActive, value: true },
      ],
    });

    const partySearch = new SearchFactory({
      objectType: m.Party,
      roleTypes: [m.Party.DisplayName],
    });

    const requestStateSearch = new SearchFactory({
      objectType: m.RequestState,
      roleTypes: [m.RequestState.Name],
    });

    const quoteStateSearch = new SearchFactory({
      objectType: m.QuoteState,
      roleTypes: [m.QuoteState.Name],
    });

    const salesOrderstateSearch = new SearchFactory({
      objectType: m.SalesOrderState,
      roleTypes: [m.SalesOrderState.Name],
    });

    const salesOrderInvoiceStateSearch = new SearchFactory({
      objectType: m.SalesOrderInvoiceState,
      roleTypes: [m.SalesOrderInvoiceState.Name],
    });

    const salesOrderShipmentStateSearch = new SearchFactory({
      objectType: m.SalesOrderShipmentState,
      roleTypes: [m.SalesOrderShipmentState.Name],
    });

    const serialisedItemSearch = new SearchFactory({
      objectType: m.SerialisedItem,
      roleTypes: [m.SerialisedItem.ItemNumber],
    });

    const salesInvoiceTypeSearch = new SearchFactory({
      objectType: m.SalesInvoiceType,
      roleTypes: [m.SalesInvoiceType.Name],
    });

    const salesInvoiceStateSearch = new SearchFactory({
      objectType: m.SalesInvoiceState,
      roleTypes: [m.SalesInvoiceState.Name],
    });

    const purchaseInvoiceTypeSearch = new SearchFactory({
      objectType: m.PurchaseInvoiceType,
      roleTypes: [m.PurchaseInvoiceType.Name],
    });

    const purchaseInvoiceStateSearch = new SearchFactory({
      objectType: m.PurchaseInvoiceState,
      roleTypes: [m.PurchaseInvoiceState.Name],
    });

    const purchaseOrderStateSearch = new SearchFactory({
      objectType: m.PurchaseOrderState,
      roleTypes: [m.PurchaseOrderState.Name],
    });

    const shipmentStateSearch = new SearchFactory({
      objectType: m.ShipmentState,
      roleTypes: [m.ShipmentState.Name],
    });

    const workEffortStateSearch = new SearchFactory({
      objectType: m.WorkEffortState,
      roleTypes: [m.WorkEffortState.Name],
    });

    const personSearch = new SearchFactory({
      objectType: m.Person,
      roleTypes: [m.Person.DisplayName, m.Person.UserName],
    });

    const fixedAssetSearch = new SearchFactory({
      objectType: m.FixedAsset,
      roleTypes: [m.FixedAsset.SearchString],
    });

    const positionTypeSearch = new SearchFactory({
      objectType: m.PositionType,
      roleTypes: [m.PositionType.Title],
    });

    const rateTypeSearch = new SearchFactory({
      objectType: m.RateType,
      roleTypes: [m.RateType.Name],
      predicates: [
        { kind: 'Equals', propertyType: m.RateType.IsActive, value: true },
      ],
    });

    const typeSearch = new SearchFactory({
      objectType: m.ProductType,
      roleTypes: [m.ProductType.Name],
    });

    const kindSearch = new SearchFactory({
      objectType: m.InventoryItemKind,
      predicates: [
        { kind: 'Equals', propertyType: m.Enumeration.IsActive, value: true },
      ],
      roleTypes: [m.InventoryItemKind.Name],
    });

    const productIdentificationSearch = new SearchFactory({
      objectType: m.ProductIdentification,
      roleTypes: [m.ProductIdentification.Identification],
    });

    define(
      m.Carrier,
      new FilterDefinition({
        kind: 'And',
        operands: [
          { kind: 'Like', roleType: m.Carrier.Name, parameter: 'name' },
        ],
      })
    );

    define(
      m.Catalogue,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            { kind: 'Like', roleType: m.Catalogue.Name, parameter: 'Name' },
            {
              kind: 'Equals',
              propertyType: m.Catalogue.CatScope,
              parameter: 'Scope',
            },
          ],
        },
        {
          Scope: {
            search: () => scopeSearch,
            display: (v: Scope) => v && v.Name,
          },
        }
      )
    );

    define(
      m.CommunicationEvent,
      new FilterDefinition({
        kind: 'And',
        operands: [
          {
            kind: 'Like',
            roleType: m.CommunicationEvent.Subject,
            parameter: 'subject',
          },
        ],
      })
    );

    define(
      m.ExchangeRate,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.ExchangeRate.FromCurrency,
              parameter: 'fromCurrency',
            },
            {
              kind: 'Equals',
              propertyType: m.ExchangeRate.ToCurrency,
              parameter: 'toCurrency',
            },
          ],
        },
        {
          fromCurrency: {
            search: () => currencySearch,
            display: (v: Currency) => v && v.IsoCode,
          },
          toCurrency: {
            search: () => currencySearch,
            display: (v: Currency) => v && v.IsoCode,
          },
        }
      )
    );

    define(
      m.Good,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            { kind: 'Like', roleType: m.Good.Name, parameter: 'name' },
            { kind: 'Like', roleType: m.Good.Keywords, parameter: 'keyword' },
            {
              kind: 'Contains',
              propertyType: m.Good.ProductCategoriesWhereProduct,
              parameter: 'category',
            },
            {
              kind: 'Contains',
              propertyType: m.Good.ProductIdentifications,
              parameter: 'identification',
            },
            {
              kind: 'Exists',
              propertyType: m.Good.SalesDiscontinuationDate,
              parameter: 'discontinued',
            },
          ],
        },
        {
          supplier: {
            search: () => Filters.allSuppliersFilter(m),
            display: (v: Organisation) => v && v.DisplayName,
          },
          manufacturer: {
            search: () => manufacturerSearch,
            display: (v: Organisation) => v && v.DisplayName,
          },
          brand: {
            search: () => brandSearch,
            display: (v: Brand) => v && v.Name,
          },
          model: {
            search: () => modelSearch,
            display: (v: Model) => v && v.Name,
          },
          kind: {
            search: () => kindSearch,
            display: (v: InventoryItemKind) => v && v.Name,
          },
          type: {
            search: () => typeSearch,
            display: (v: ProductType) => v && v.Name,
          },
          category: {
            search: () => categorySearch,
            display: (v: PartCategory) => v && v.Name,
          },
          identification: {
            search: () => productIdentificationSearch,
            display: (v: ProductIdentification) => v && v.Identification,
          },
          inStock: {
            search: () => facilitySearch,
            display: (v: Facility) => v && v.Name,
          },
          outOfStock: {
            search: () => facilitySearch,
            display: (v: Facility) => v && v.Name,
          },
        }
      )
    );

    define(
      m.NonUnifiedPart,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Or',
              operands: [
                { kind: 'Like', roleType: m.Part.Name, parameter: 'name' },
                {
                  kind: 'ContainedIn',
                  propertyType: m.Part.LocalisedNames,
                  extent: {
                    kind: 'Filter',
                    objectType: m.LocalisedText,
                    predicate: {
                      kind: 'Like',
                      roleType: m.LocalisedText.Text,
                      parameter: 'name',
                    },
                  },
                },
              ],
            },
            { kind: 'Like', roleType: m.Part.Keywords, parameter: 'keyword' },
            { kind: 'Like', roleType: m.Part.HsCode, parameter: 'hsCode' },
            {
              kind: 'Contains',
              propertyType: m.Part.ProductIdentifications,
              parameter: 'identification',
            },
            {
              kind: 'Contains',
              propertyType: m.Part.SuppliedBy,
              parameter: 'supplier',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Part.SupplierOfferingsWherePart,
              extent: {
                kind: 'Filter',
                objectType: m.SupplierOffering,
                predicate: {
                  kind: 'Like',
                  roleType: m.SupplierOffering.SupplierProductId,
                  parameter: 'supplierReference',
                },
              },
            },
            {
              kind: 'Equals',
              propertyType: m.Part.ManufacturedBy,
              parameter: 'manufacturer',
            },
            { kind: 'Equals', propertyType: m.Part.Brand, parameter: 'brand' },
            { kind: 'Equals', propertyType: m.Part.Model, parameter: 'model' },
            {
              kind: 'Equals',
              propertyType: m.Part.InventoryItemKind,
              parameter: 'kind',
            },
            {
              kind: 'Equals',
              propertyType: m.Part.ProductType,
              parameter: 'type',
            },
            {
              kind: 'Contains',
              propertyType: m.NonUnifiedPart.PartCategoriesWherePart,
              parameter: 'category',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Part.InventoryItemsWherePart,
              extent: {
                kind: 'Filter',
                objectType: m.NonSerialisedInventoryItem,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.InventoryItem.Facility,
                  parameter: 'inStock',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Part.InventoryItemsWherePart,
              extent: {
                kind: 'Filter',
                objectType: m.NonSerialisedInventoryItem,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.InventoryItem.Facility,
                  parameter: 'outOfStock',
                },
              },
            },
          ],
        },
        {
          category: {
            search: () => categorySearch,
            display: (v: ProductCategory) => v && v.Name,
          },
          identification: {
            search: () => productIdSearch,
            display: (v: ProductIdentification) => v && v.Identification,
          },
          brand: {
            search: () => brandSearch,
            display: (v: Brand) => v && v.Name,
          },
          model: {
            search: () => modelSearch,
            display: (v: Model) => v && v.Name,
          },
        }
      )
    );

    define(
      m.Organisation,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            { kind: 'Like', roleType: m.Organisation.Name, parameter: 'name' },
            {
              kind: 'ContainedIn',
              propertyType: m.Organisation.SupplierRelationshipsWhereSupplier,
              extent: {
                kind: 'Filter',
                objectType: m.SupplierRelationship,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.SupplierRelationship.InternalOrganisation,
                  parameter: 'supplierFor',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.CustomerRelationshipsWhereCustomer,
              extent: {
                kind: 'Filter',
                objectType: m.CustomerRelationship,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.CustomerRelationship.InternalOrganisation,
                  parameter: 'customerAt',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.PartyContactMechanismsWhereParty,
              extent: {
                kind: 'Filter',
                objectType: m.PartyContactMechanism,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PartyContactMechanism.ContactMechanism,
                  extent: {
                    kind: 'Filter',
                    objectType: m.PostalAddress,
                    predicate: {
                      kind: 'ContainedIn',
                      propertyType: m.PostalAddress.Country,
                      parameter: 'country',
                    },
                  },
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.PartyContactMechanismsWhereParty,
              extent: {
                kind: 'Filter',
                objectType: m.PartyContactMechanism,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PartyContactMechanism.ContactMechanism,
                  extent: {
                    kind: 'Filter',
                    objectType: m.PostalAddress,
                    predicate: {
                      kind: 'Like',
                      roleType: m.PostalAddress.Locality,
                      parameter: 'city',
                    },
                  },
                },
              },
            },
          ],
        },
        {
          customerAt: {
            search: () => Filters.internalOrganisationsFilter(m),
            initialValue: () => this.internalOrganisationId.value,
            display: (v: Organisation) => v && v.Name,
          },
          supplierFor: {
            search: () => Filters.internalOrganisationsFilter(m),
            initialValue: () => this.internalOrganisationId.value,
            display: (v: Organisation) => v && v.Name,
          },
          country: {
            search: () => countrySearch,
            display: (v: Country) => v && v.Name,
          },
        }
      )
    );

    define(
      m.Part,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            { kind: 'Like', roleType: m.Part.Name, parameter: 'name' },
            { kind: 'Like', roleType: m.Part.Keywords, parameter: 'keyword' },
            { kind: 'Like', roleType: m.Part.HsCode, parameter: 'hsCode' },
            {
              kind: 'Contains',
              propertyType: m.Part.ProductIdentifications,
              parameter: 'identification',
            },
            {
              kind: 'Contains',
              propertyType: m.Part.SuppliedBy,
              parameter: 'supplier',
            },
            {
              kind: 'Equals',
              propertyType: m.Part.ManufacturedBy,
              parameter: 'manufacturer',
            },
            { kind: 'Equals', propertyType: m.Part.Brand, parameter: 'brand' },
            { kind: 'Equals', propertyType: m.Part.Model, parameter: 'model' },
            {
              kind: 'Equals',
              propertyType: m.Part.InventoryItemKind,
              parameter: 'kind',
            },
            {
              kind: 'Equals',
              propertyType: m.Part.ProductType,
              parameter: 'type',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Part.InventoryItemsWherePart,
              extent: {
                kind: 'Filter',
                objectType: m.InventoryItem,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.InventoryItem.Facility,
                  parameter: 'facility',
                },
              },
            },
          ],
        },
        {
          supplier: {
            search: () =>
              Filters.suppliersFilter(m, internalOrganisationId.value),
            display: (v: Organisation) => v && v.DisplayName,
          },
          manufacturer: {
            search: () => manufacturerSearch,
            display: (v: Organisation) => v && v.DisplayName,
          },
          brand: {
            search: () => brandSearch,
            display: (v: Brand) => v && v && v.Name,
          },
          model: { search: () => modelSearch, display: (v: Model) => v.Name },
          kind: {
            search: () => inventoryItemKindSearch,
            display: (v: InventoryItemKind) => v && v.Name,
          },
          type: {
            search: () => productTypeSearch,
            display: (v: ProductType) => v && v.Name,
          },
          identification: {
            search: () => productIdSearch,
            display: (v: ProductIdentification) => v && v.Identification,
          },
          facility: {
            search: () => facilitySearch,
            display: (v: Facility) => v && v.Name,
          },
        }
      )
    );

    define(
      m.Person,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.Person.FirstName,
              parameter: 'firstName',
            },
            {
              kind: 'Like',
              roleType: m.Person.LastName,
              parameter: 'lastName',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.PartyContactMechanismsWhereParty,
              extent: {
                kind: 'Filter',
                objectType: m.PartyContactMechanism,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PartyContactMechanism.ContactMechanism,
                  extent: {
                    kind: 'Filter',
                    objectType: m.PostalAddress,
                    predicate: {
                      kind: 'ContainedIn',
                      propertyType: m.PostalAddress.Country,
                      parameter: 'country',
                    },
                  },
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.PartyContactMechanismsWhereParty,
              extent: {
                kind: 'Filter',
                objectType: m.PartyContactMechanism,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PartyContactMechanism.ContactMechanism,
                  extent: {
                    kind: 'Filter',
                    objectType: m.PostalAddress,
                    predicate: {
                      kind: 'Like',
                      roleType: m.PostalAddress.Locality,
                      parameter: 'city',
                    },
                  },
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Party.CustomerRelationshipsWhereCustomer,
              extent: {
                kind: 'Filter',
                objectType: m.CustomerRelationship,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.CustomerRelationship.InternalOrganisation,
                  parameter: 'customerAt',
                },
              },
            },
          ],
        },
        {
          customerAt: {
            search: () => Filters.internalOrganisationsFilter(m),
            display: (v: Organisation) => v && v.Name,
          },
          country: {
            search: () => countrySearch,
            display: (v: Country) => v && v.Name,
          },
        }
      )
    );

    define(
      m.PositionType,
      new FilterDefinition({
        kind: 'And',
        operands: [
          { kind: 'Like', roleType: m.PositionType.Title, parameter: 'title' },
        ],
      })
    );

    define(
      m.PositionTypeRate,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Contains',
              propertyType:
                m.PositionTypeRate.PositionTypesWherePositionTypeRate,
              parameter: 'positionType',
            },
            {
              kind: 'Equals',
              propertyType: m.PositionTypeRate.RateType,
              parameter: 'rateType',
            },
          ],
        },
        {
          active: { initialValue: true },
          positionType: {
            search: () => positionTypeSearch,
            display: (v: PositionType) => v && v.Title,
          },
          rateType: {
            search: () => rateTypeSearch,
            display: (v: RateType) => v && v.Name,
          },
        }
      )
    );

    define(
      m.ProductCategory,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.ProductCategory.Name,
              parameter: 'name',
            },
            {
              kind: 'Equals',
              propertyType: m.ProductCategory.CatScope,
              parameter: 'scope',
            },
            {
              kind: 'Contains',
              propertyType: m.ProductCategory.Products,
              parameter: 'product',
            },
          ],
        },
        {
          scope: {
            search: () => scopeSearch,
            display: (v: Scope) => v && v.Name,
          },
          product: {
            search: () => productSearch,
            display: (v: Good) => v && v.Name,
          },
        }
      )
    );

    define(
      m.ProductQuote,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.ProductQuote.QuoteState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.ProductQuote.Receiver,
              parameter: 'to',
            },
          ],
        },
        {
          active: { initialValue: true },
          state: {
            search: () => quoteStateSearch,
            display: (v: QuoteState) => v && v.Name,
          },
          to: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
        }
      )
    );

    define(
      m.ProductType,
      new FilterDefinition({
        kind: 'And',
        operands: [
          { kind: 'Like', roleType: m.ProductType.Name, parameter: 'name' },
        ],
      })
    );

    define(
      m.PurchaseInvoice,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.PurchaseInvoice.InvoiceNumber,
              parameter: 'number',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseInvoice.PurchaseInvoiceState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseInvoice.PurchaseInvoiceType,
              parameter: 'type',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseInvoice.BilledFrom,
              parameter: 'supplier',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.PurchaseInvoice.PurchaseInvoiceItems,
              extent: {
                kind: 'Filter',
                objectType: m.PurchaseInvoiceItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PurchaseInvoiceItem.Part,
                  parameter: 'sparePart',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.PurchaseInvoice.PurchaseInvoiceItems,
              extent: {
                kind: 'Filter',
                objectType: m.PurchaseInvoiceItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PurchaseInvoiceItem.SerialisedItem,
                  parameter: 'serialisedItem',
                },
              },
            },
          ],
        },
        {
          type: {
            search: () => purchaseInvoiceTypeSearch,
            display: (v: PurchaseInvoiceType) => v && v.Name,
          },
          state: {
            search: () => purchaseInvoiceStateSearch,
            display: (v: PurchaseInvoiceState) => v && v.Name,
          },
          supplier: {
            search: () =>
              Filters.suppliersFilter(m, internalOrganisationId.value),
            display: (v: Party) => v && v.DisplayName,
          },
          sparePart: {
            search: () => partSearch,
            display: (v: Part) => v && v.Name,
          },
          serialisedItem: {
            search: () => serialisedItemSearch,
            display: (v: SerialisedItem) => v && v.ItemNumber,
          },
        }
      )
    );

    define(
      m.PurchaseOrder,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.PurchaseOrder.OrderNumber,
              parameter: 'number',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseOrder.CustomerReference,
              parameter: 'customerReference',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseOrder.PurchaseOrderState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.PurchaseOrder.TakenViaSupplier,
              parameter: 'supplier',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.PurchaseOrder.PurchaseOrderItems,
              extent: {
                kind: 'Filter',
                objectType: m.PurchaseOrderItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PurchaseOrderItem.Part,
                  parameter: 'sparePart',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.PurchaseOrder.PurchaseOrderItems,
              extent: {
                kind: 'Filter',
                objectType: m.PurchaseOrderItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.PurchaseOrderItem.SerialisedItem,
                  parameter: 'serialisedItem',
                },
              },
            },
          ],
        },
        {
          active: { initialValue: true },
          state: {
            search: () => purchaseOrderStateSearch,
            display: (v: PurchaseOrderState) => v && v.Name,
          },
          supplier: {
            search: () =>
              Filters.suppliersFilter(m, internalOrganisationId.value),
            display: (v: Party) => v && v.DisplayName,
          },
          sparePart: {
            search: () => partSearch,
            display: (v: Part) => v && v.Name,
          },
          serialisedItem: {
            search: () => serialisedItemSearch,
            display: (v: SerialisedItem) => v && v.ItemNumber,
          },
        }
      )
    );

    define(
      m.RequestForQuote,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.Request.RequestState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.Request.Originator,
              parameter: 'from',
            },
          ],
        },
        {
          active: { initialValue: true },
          state: {
            search: () => requestStateSearch,
            display: (v: RequestState) => v && v.Name,
          },
          from: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
        }
      )
    );

    define(
      m.SalesInvoice,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.SalesInvoiceType,
              parameter: 'type',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.InvoiceNumber,
              parameter: 'number',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.CustomerReference,
              parameter: 'customerReference',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.SalesInvoiceState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.ShipToCustomer,
              parameter: 'shipTo',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.BillToCustomer,
              parameter: 'billTo',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.ShipToEndCustomer,
              parameter: 'shipToEndCustomer',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.BillToEndCustomer,
              parameter: 'billToEndCustomer',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesInvoice.IsRepeatingInvoice,
              parameter: 'repeating',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SalesInvoice.SalesInvoiceItems,
              extent: {
                kind: 'Filter',
                objectType: m.SalesInvoiceItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.SalesInvoiceItem.Product,
                  parameter: 'product',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SalesInvoice.SalesInvoiceItems,
              extent: {
                kind: 'Filter',
                objectType: m.SalesInvoiceItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.SalesInvoiceItem.SerialisedItem,
                  parameter: 'serialisedItem',
                },
              },
            },
          ],
        },
        {
          repeating: { initialValue: true },
          active: { initialValue: true },
          type: {
            search: () => salesInvoiceTypeSearch,
            display: (v: SalesInvoiceType) => v && v.Name,
          },
          state: {
            search: () => salesInvoiceStateSearch,
            display: (v: SalesInvoiceState) => v && v.Name,
          },
          shipTo: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          billTo: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          shipToEndCustomer: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          billToEndCustomer: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          product: {
            search: () => productSearch,
            display: (v: Product) => v && v.Name,
          },
          serialisedItem: {
            search: () => serialisedItemSearch,
            display: (v: SerialisedItem) => v && v.ItemNumber,
          },
        }
      )
    );

    define(
      m.SalesOrder,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.OrderNumber,
              parameter: 'number',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.CustomerReference,
              parameter: 'customerReference',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.SalesOrderState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.SalesOrderInvoiceState,
              parameter: 'invoiceState',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.SalesOrderShipmentState,
              parameter: 'shipmentState',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.ShipToCustomer,
              parameter: 'shipTo',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.BillToCustomer,
              parameter: 'billTo',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.ShipToEndCustomer,
              parameter: 'shipToEndCustomer',
            },
            {
              kind: 'Equals',
              propertyType: m.SalesOrder.BillToEndCustomer,
              parameter: 'billToEndCustomer',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SalesOrder.SalesOrderItems,
              extent: {
                kind: 'Filter',
                objectType: m.SalesOrderItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.SalesOrderItem.Product,
                  parameter: 'product',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SalesOrder.SalesOrderItems,
              extent: {
                kind: 'Filter',
                objectType: m.SalesOrderItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.SalesOrderItem.SerialisedItem,
                  parameter: 'serialisedItem',
                },
              },
            },
          ],
        },
        {
          active: { initialValue: true },
          state: {
            search: () => salesOrderstateSearch,
            display: (v: SalesOrderState) => v && v.Name,
          },
          invoiceState: {
            search: () => salesOrderInvoiceStateSearch,
            display: (v: SalesInvoiceState) => v && v.Name,
          },
          shipmentState: {
            search: () => salesOrderShipmentStateSearch,
            display: (v: ShipmentState) => v && v.Name,
          },
          shipTo: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          billTo: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          shipToEndCustomer: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          billToEndCustomer: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          product: {
            search: () => productSearch,
            display: (v: Product) => v && v.Name,
          },
          serialisedItem: {
            search: () => serialisedItemSearch,
            display: (v: SerialisedItem) => v && v.ItemNumber,
          },
        }
      )
    );

    define(
      m.SerialisedItem,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.SerialisedItem.ItemNumber,
              parameter: 'id',
            },
            {
              kind: 'Like',
              roleType: m.SerialisedItem.Name,
              parameter: 'name',
            },
            {
              kind: 'Like',
              roleType: m.SerialisedItem.Keywords,
              parameter: 'keyword',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.OnQuote,
              parameter: 'onQuote',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.OnSalesOrder,
              parameter: 'onSalesOrder',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.OnWorkEffort,
              parameter: 'onWorkEffort',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.SerialisedItemAvailability,
              parameter: 'availability',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.SerialisedItemState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.Ownership,
              parameter: 'ownership',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.SuppliedBy,
              parameter: 'suppliedby',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.RentedBy,
              parameter: 'rentedby',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItem.OwnedBy,
              parameter: 'ownedby',
            },
            {
              kind: 'Like',
              roleType: m.SerialisedItem.ProductCategoriesDisplayName,
              parameter: 'category',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SerialisedItem.PartWhereSerialisedItem,
              extent: {
                kind: 'Filter',
                objectType: m.Part,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.Part.Brand,
                  parameter: 'brand',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SerialisedItem.PartWhereSerialisedItem,
              extent: {
                kind: 'Filter',
                objectType: m.Part,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.Part.Model,
                  parameter: 'model',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.SerialisedItem.PartWhereSerialisedItem,
              extent: {
                kind: 'Filter',
                objectType: m.UnifiedGood,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.UnifiedGood.ProductType,
                  parameter: 'productType',
                },
              },
            },
          ],
        },
        {
          active: { initialValue: true },
          onQuote: { initialValue: true },
          onSalesOrder: { initialValue: true },
          onWorkEffort: { initialValue: true },
          state: {
            search: () => serialisedItemStateSearch,
            display: (v: SerialisedItemState) => v && v.Name,
          },
          availability: {
            search: () => serialisedItemAvailabilitySearch,
            display: (v: SerialisedItemAvailability) => v && v.Name,
          },
          ownership: {
            search: () => ownershipSearch,
            display: (v: Ownership) => v && v.Name,
          },
          suppliedby: {
            search: () => Filters.allSuppliersFilter(m),
            display: (v: Organisation) => v && v.Name,
          },
          ownedby: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          rentedby: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          brand: {
            search: () => brandSearch,
            display: (v: Brand) => v && v.Name,
          },
          model: {
            search: () => modelSearch,
            display: (v: Model) => v && v.Name,
          },
          productType: {
            search: () => productTypeSearch,
            display: (v: ProductType) => v && v.Name,
          },
        }
      )
    );

    define(
      m.SerialisedItemCharacteristic,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.SerialisedItemCharacteristicType.Name,
              parameter: 'name',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItemCharacteristicType.IsActive,
              parameter: 'active',
            },
            {
              kind: 'Equals',
              propertyType: m.SerialisedItemCharacteristicType.UnitOfMeasure,
              parameter: 'uom',
            },
          ],
        },
        {
          active: { initialValue: true },
          uom: {
            search: () => uomSearch,
            display: (v: IUnitOfMeasure) => v && v.Name,
          },
        }
      )
    );

    define(
      m.Shipment,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.Shipment.ShipmentNumber,
              parameter: 'number',
            },
            {
              kind: 'Equals',
              propertyType: m.Shipment.ShipmentState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.Shipment.ShipFromParty,
              parameter: 'shipFrom',
            },
            {
              kind: 'Equals',
              propertyType: m.Shipment.ShipToParty,
              parameter: 'shipTo',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.Shipment.ShipmentItems,
              extent: {
                kind: 'Filter',
                objectType: m.ShipmentItem,
                predicate: {
                  kind: 'ContainedIn',
                  propertyType: m.ShipmentItem.Part,
                  parameter: 'part',
                },
              },
            },
          ],
        },
        {
          active: { initialValue: true },
          state: {
            search: () => shipmentStateSearch,
            display: (v: ShipmentState) => v && v.Name,
          },
          shipFrom: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          shipTo: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
        }
      )
    );

    define(
      m.UnifiedGood,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            { kind: 'Like', roleType: m.UnifiedGood.Name, parameter: 'name' },
            {
              kind: 'Like',
              roleType: m.UnifiedGood.Keywords,
              parameter: 'keyword',
            },
            {
              kind: 'Contains',
              propertyType: m.UnifiedGood.ProductCategoriesWhereProduct,
              parameter: 'category',
            },
            {
              kind: 'Contains',
              propertyType: m.UnifiedGood.ProductIdentifications,
              parameter: 'identification',
            },
            {
              kind: 'Equals',
              propertyType: m.UnifiedGood.Brand,
              parameter: 'brand',
            },
            {
              kind: 'Equals',
              propertyType: m.UnifiedGood.Model,
              parameter: 'model',
            },
            {
              kind: 'Exists',
              propertyType: m.UnifiedGood.SalesDiscontinuationDate,
              parameter: 'discontinued',
            },
            {
              kind: 'Exists',
              propertyType: m.UnifiedGood.Photos,
              parameter: 'photos',
            },
          ],
        },
        {
          category: {
            search: () => categorySearch,
            display: (v: ProductCategory) => v && v.DisplayName,
          },
          identification: {
            search: () => productIdSearch,
            display: (v: ProductIdentification) => v && v.Identification,
          },
          brand: {
            search: () => brandSearch,
            display: (v: Brand) => v && v.Name,
          },
          model: {
            search: () => modelSearch,
            display: (v: Model) => v && v.Name,
          },
        }
      )
    );

    define(
      m.WorkEffort,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.WorkEffort.WorkEffortState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.WorkEffort.Customer,
              parameter: 'customer',
            },
            {
              kind: 'Equals',
              propertyType: m.WorkEffort.ExecutedBy,
              parameter: 'ExecutedBy',
            },
            {
              kind: 'Like',
              roleType: m.WorkEffort.WorkEffortNumber,
              parameter: 'Number',
            },
            { kind: 'Like', roleType: m.WorkEffort.Name, parameter: 'Name' },
            {
              kind: 'Like',
              roleType: m.WorkEffort.Description,
              parameter: 'Description',
            },
            {
              kind: 'ContainedIn',
              propertyType:
                m.WorkEffort.WorkEffortPartyAssignmentsWhereAssignment,
              extent: {
                kind: 'Filter',
                objectType: m.WorkEffortPartyAssignment,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.WorkEffortPartyAssignment.Party,
                  parameter: 'worker',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType:
                m.WorkEffort.WorkEffortFixedAssetAssignmentsWhereAssignment,
              extent: {
                kind: 'Filter',
                objectType: m.WorkEffortFixedAssetAssignment,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.WorkEffortFixedAssetAssignment.FixedAsset,
                  parameter: 'equipment',
                },
              },
            },
          ],
        },
        {
          state: {
            search: () => workEffortStateSearch,
            display: (v: WorkEffortState) => v && v.Name,
          },
          customer: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          ExecutedBy: {
            search: () => partySearch,
            display: (v: Party) => v && v.DisplayName,
          },
          worker: {
            search: () => personSearch,
            display: (v: Person) => v && v.DisplayName,
          },
          equipment: {
            search: () => fixedAssetSearch,
            display: (v: FixedAsset) => v && v.DisplayName,
          },
        }
      )
    );

    define(
      m.WorkRequirement,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.WorkRequirement.RequirementState,
              parameter: 'state',
            },
            {
              kind: 'Equals',
              propertyType: m.WorkRequirement.Priority,
              parameter: 'priority',
            },
            {
              kind: 'Like',
              roleType: m.WorkRequirement.RequirementNumber,
              parameter: 'Number',
            },
            {
              kind: 'Like',
              roleType: m.WorkRequirement.Location,
              parameter: 'Location',
            },
            {
              kind: 'Equals',
              propertyType: m.WorkRequirement.FixedAsset,
              parameter: 'equipment',
            },
            {
              kind: 'ContainedIn',
              propertyType: m.WorkRequirement.FixedAsset,
              extent: {
                kind: 'Filter',
                objectType: m.SerialisedItem,
                predicate: {
                  kind: 'Like',
                  roleType: m.SerialisedItem.CustomerReferenceNumber,
                  parameter: 'fleetcode',
                },
              },
            },
          ],
        },
        {
          state: {
            search: () => requirementStateSearch,
            display: (v: RequirementState) => v && v.Name,
          },
          priority: {
            search: () => prioritySearch,
            display: (v: Priority) => v && v.Name,
          },
          equipment: {
            search: () => serialisedItemSearch,
            display: (v: SerialisedItem) => v && v.DisplayName,
          },
        }
      )
    );
  }

  filter(composite: Composite): Filter {
    let filter = this.filterByComposite.get(composite);
    if (filter == null) {
      const filterDefinition = this.filterDefinitionByComposite.get(composite);
      filter = new Filter(filterDefinition);
      this.filterByComposite.set(composite, filter);
    }

    return filter;
  }
}
