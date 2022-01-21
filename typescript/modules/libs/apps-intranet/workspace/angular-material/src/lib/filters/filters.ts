import { SearchFactory } from '@allors/base/workspace/angular/foundation';
import { And } from '@allors/system/workspace/domain';
import { M, TreeBuilder } from '@allors/default/workspace/meta';

export class Filters {
  static goodsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Good,
      roleTypes: [m.Good.Name, m.Good.SearchString],
    });
  }

  static serialisedgoodsFilter(m: M) {
    return new SearchFactory({
      objectType: m.UnifiedGood,
      roleTypes: [m.UnifiedGood.Name, m.UnifiedGood.SearchString],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.UnifiedGood.InventoryItemKind,
          extent: {
            kind: 'Filter',
            objectType: m.InventoryItemKind,
            predicate: {
              kind: 'Equals',
              propertyType: m.InventoryItemKind.UniqueId,
              value: '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae',
            },
          },
        });
      },
    });
  }

  static partsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Part,
      roleTypes: [m.Part.Name, m.Part.SearchString],
    });
  }

  static nonUnifiedPartsFilter(m: M) {
    return new SearchFactory({
      objectType: m.NonUnifiedPart,
      roleTypes: [m.NonUnifiedPart.Name, m.NonUnifiedPart.SearchString],
    });
  }

  static unifiedGoodsFilter(m: M, treeFactory: TreeBuilder) {
    return new SearchFactory({
      objectType: m.UnifiedGood,
      roleTypes: [m.UnifiedGood.Name, m.UnifiedGood.SearchString],
      include: treeFactory.UnifiedGood({
        SerialisedItems: {},
        PartWeightedAverage: {},
      }),
    });
  }

  static serialisedItemsFilter(m: M) {
    return new SearchFactory({
      objectType: m.SerialisedItem,
      roleTypes: [m.SerialisedItem.Name, m.SerialisedItem.SearchString],
    });
  }

  static customersFilter(m: M, internalOrganisationId: number) {
    return new SearchFactory({
      objectType: m.Party,
      roleTypes: [m.Party.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Party.CustomerRelationshipsWhereCustomer,
          extent: {
            kind: 'Filter',
            objectType: m.CustomerRelationship,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.CustomerRelationship.InternalOrganisation,
                  value: internalOrganisationId,
                },
                {
                  kind: 'LessThan',
                  roleType: m.CustomerRelationship.FromDate,
                  value: new Date(),
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: m.CustomerRelationship.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: m.CustomerRelationship.ThroughDate,
                      value: new Date(),
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  static suppliersFilter(m: M, internalOrganisationId: number) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Organisation.SupplierRelationshipsWhereSupplier,
          extent: {
            kind: 'Filter',
            objectType: m.SupplierRelationship,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.SupplierRelationship.InternalOrganisation,
                  value: internalOrganisationId,
                },
                {
                  kind: 'LessThan',
                  roleType: m.SupplierRelationship.FromDate,
                  value: new Date(),
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: m.SupplierRelationship.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: m.SupplierRelationship.ThroughDate,
                      value: new Date(),
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  static allSuppliersFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Organisation.SupplierRelationshipsWhereSupplier,
          extent: {
            kind: 'Filter',
            objectType: m.SupplierRelationship,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'LessThan',
                  roleType: m.SupplierRelationship.FromDate,
                  value: new Date(),
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: m.SupplierRelationship.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: m.SupplierRelationship.ThroughDate,
                      value: new Date(),
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  static subContractorsFilter(m: M, internalOrganisationId: number) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType:
            m.Organisation.SubContractorRelationshipsWhereSubContractor,
          extent: {
            kind: 'Filter',
            objectType: m.SubContractorRelationship,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.SubContractorRelationship.Contractor,
                  value: internalOrganisationId,
                },
                {
                  kind: 'LessThan',
                  roleType: m.SubContractorRelationship.FromDate,
                  value: new Date(),
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: m.SubContractorRelationship.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: m.SubContractorRelationship.ThroughDate,
                      value: new Date(),
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  static employeeFilter(m: M, internalOrganisationId: number) {
    return new SearchFactory({
      objectType: m.Person,
      roleTypes: [m.Person.DisplayName, m.Person.UserName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Person.EmploymentsWhereEmployee,
          extent: {
            kind: 'Filter',
            objectType: m.Employment,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.Employment.Employer,
                  value: internalOrganisationId,
                },
                {
                  kind: 'LessThan',
                  roleType: m.Employment.FromDate,
                  value: new Date(),
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: m.Employment.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: m.Employment.ThroughDate,
                      value: new Date(),
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  static organisationsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
    });
  }

  static internalOrganisationsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        });
      },
    });
  }

  static manufacturersFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.DisplayName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'Equals',
          propertyType: m.Organisation.IsManufacturer,
          value: true,
        });
      },
    });
  }

  static peopleFilter(m: M) {
    return new SearchFactory({
      objectType: m.Person,
      roleTypes: [m.Person.DisplayName],
    });
  }

  static partiesFilter(m: M) {
    return new SearchFactory({
      objectType: m.Party,
      roleTypes: [m.Party.DisplayName],
    });
  }

  static workEffortsFilter(m: M) {
    return new SearchFactory({
      objectType: m.WorkEffort,
      roleTypes: [m.WorkEffort.Name],
    });
  }
}
