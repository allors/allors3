import { SearchFactory } from '@allors/workspace/angular/base';
import { And } from '@allors/workspace/domain/system';
import { M, TreeBuilder } from '@allors/workspace/meta/default';

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
            predicate: { kind: 'Equals', propertyType: m.InventoryItemKind.UniqueId, value: '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae' },
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
      include: treeFactory.UnifiedGood({ SerialisedItems: {}, PartWeightedAverage: {} }),
    });
  }

  static serialisedItemsFilter(m: M) {
    return new SearchFactory({
      objectType: m.SerialisedItem,
      roleTypes: [m.SerialisedItem.Name, m.SerialisedItem.SearchString],
    });
  }

  static customersFilter(m: M, internalOrganisationId: string) {
    return new SearchFactory({
      objectType: m.Party,
      roleTypes: [m.Party.PartyName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Party.CustomerRelationshipsWhereCustomer,
          extent: {
            kind: 'Filter',
            objectType: m.CustomerRelationship,
            predicate: {
              kind: 'Equals',
              propertyType: m.CustomerRelationship.InternalOrganisation,
              value: internalOrganisationId,
            },
          },
        });
      },
    });
  }

  static suppliersFilter(m: M, internalOrganisationId: string) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.PartyName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Organisation.SupplierRelationshipsWhereSupplier,
          extent: {
            kind: 'Filter',
            objectType: m.SupplierRelationship,
            predicate: {
              kind: 'Equals',
              propertyType: m.SupplierRelationship.InternalOrganisation,
              value: internalOrganisationId,
            },
          },
        });
      },
    });
  }

  static allSuppliersFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.PartyName],
      post: (predicate: And) => {
        predicate.operands.push({ kind: 'ContainedIn', propertyType: m.Organisation.SupplierRelationshipsWhereSupplier, extent: { kind: 'Filter', objectType: m.SupplierRelationship } });
      },
    });
  }

  static subContractorsFilter(m: M, internalOrganisationId: string) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.PartyName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Organisation.SubContractorRelationshipsWhereSubContractor,
          extent: { kind: 'Filter', objectType: m.SubContractorRelationship, predicate: { kind: 'Equals', propertyType: m.SubContractorRelationship.Contractor, value: internalOrganisationId } },
        });
      },
    });
  }

  static employeeFilter(m: M, internalOrganisationId: string) {
    return new SearchFactory({
      objectType: m.Person,
      roleTypes: [m.Person.PartyName, m.Person.UserName],
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: m.Person.EmploymentsWhereEmployee,
          extent: { kind: 'Filter', objectType: m.Employment, predicate: { kind: 'Equals', propertyType: m.Employment.Employer, value: internalOrganisationId } },
        });
      },
    });
  }

  static organisationsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.PartyName],
    });
  }

  static internalOrganisationsFilter(m: M) {
    return new SearchFactory({
      objectType: m.Organisation,
      roleTypes: [m.Organisation.PartyName],
      post: (predicate: And) => {
        predicate.operands.push({ kind: 'Equals', propertyType: m.Organisation.IsInternalOrganisation, value: true });
      },
    });
  }

  static peopleFilter(m: M) {
    return new SearchFactory({
      objectType: m.Person,
      roleTypes: [m.Person.PartyName],
    });
  }

  static partiesFilter(m: M) {
    return new SearchFactory({
      objectType: m.Party,
      roleTypes: [m.Party.PartyName],
    });
  }

  static workEffortsFilter(m: M) {
    return new SearchFactory({
      objectType: m.WorkEffort,
      roleTypes: [m.WorkEffort.Name],
    });
  }
}
