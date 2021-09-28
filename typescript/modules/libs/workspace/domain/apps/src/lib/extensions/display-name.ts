import { oneLine, stripIndents, inlineLists } from 'common-tags';
import {
  Organisation,
  Person,
  AutomatedAgent,
  EmailAddress,
  PartCategory,
  Party,
  PostalAddress,
  ProductCategory,
  PurchaseOrder,
  PurchaseOrderItem,
  SerialisedItem,
  SerialisedItemCharacteristicType,
  TelecommunicationsNumber,
  UnifiedGood,
  WebAddress,
  WorkEffortPartyAssignment,
} from '../generated';
import { tags } from '@allors/workspace/meta/apps';

export function displayName(
  item:
    | Party
    | EmailAddress
    | PartCategory
    | PostalAddress
    | ProductCategory
    | PurchaseOrder
    | PurchaseOrderItem
    | SerialisedItem
    | SerialisedItemCharacteristicType
    | TelecommunicationsNumber
    | UnifiedGood
    | WebAddress
    | WorkEffortPartyAssignment
): string {
  switch (item.strategy.cls.tag) {
    case tags.Person:
      return displayNameForPerson(item as Person);
    case tags.Organisation:
      return (item as Organisation).Name ?? 'N/A';
    case tags.AutomatedAgent:
      return (item as AutomatedAgent).UserName ?? 'N/A';
    case tags.EmailAddress:
      return (item as EmailAddress).ElectronicAddressString ?? 'N/A';
    case tags.PartCategory:
      return displayNameForPartCategory(item as PartCategory);
    case tags.PostalAddress:
      return displayNameForPostalAddress(item as PostalAddress);
    case tags.ProductCategory:
      return displayNameForProductCategory(item as ProductCategory);
    case tags.PurchaseOrder:
      return displayNameForPurchaseOrder(item as PurchaseOrder);
    case tags.PurchaseOrderItem:
      return displayNameForPurchaseOrderItem(item as PurchaseOrderItem);
    case tags.SerialisedItem:
      return displayNameForSerialisedItem(item as SerialisedItem);
    case tags.SerialisedItemCharacteristicType:
      return displayNameForSerialisedItemCharacteristicType(item as SerialisedItemCharacteristicType);
    case tags.TelecommunicationsNumber:
      return displayNameForTelecommunicationsNumber(item as TelecommunicationsNumber);
    case tags.UnifiedGood:
      return displayNameForUnifiedGood(item as UnifiedGood);
    case tags.WebAddress:
      return displayNameForWebAddress(item as WebAddress);
    case tags.WorkEffortPartyAssignment:
      return displayNameForWorkEffortPartyAssignment(item as WorkEffortPartyAssignment);
  }
}

function displayNameForPerson(person: Person): string {
  return person.FirstName + ' ' + person.LastName;
}

function displayNameForPartCategory(partCategory: PartCategory): string {
  const selfAndPrimaryAncestors = [partCategory];
  let ancestor: PartCategory | null = partCategory;
  while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
    selfAndPrimaryAncestors.push(ancestor);
    ancestor = ancestor.PrimaryParent;
  }

  selfAndPrimaryAncestors.reverse();
  const displayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
  return displayName;
}

function displayNameForPostalAddress(postalAddress: PostalAddress): string {
  return stripIndents`
  ${[postalAddress.Address1, postalAddress.Address2, postalAddress.Address3].filter((v) => v).map((v) => oneLine`${v}`)}
  ${inlineLists`${[postalAddress.PostalCode, postalAddress.Locality].filter((v) => v)}`}
  ${postalAddress.Country?.Name ?? ''}
  `;
}

function displayNameForProductCategory(productCategory: ProductCategory): string {
  const selfAndPrimaryAncestors = [productCategory];
  let ancestor: ProductCategory | null = productCategory;
  while (ancestor != null && selfAndPrimaryAncestors.indexOf(ancestor) < 0) {
    selfAndPrimaryAncestors.push(ancestor);
    ancestor = ancestor.PrimaryParent;
  }

  selfAndPrimaryAncestors.reverse();
  const displayName = selfAndPrimaryAncestors.map((v) => v.Name).join('/');
  return displayName;
}

function displayNameForPurchaseOrder(purchaseOrder: PurchaseOrder): string {
  return (purchaseOrder.OrderNumber ?? '') + (purchaseOrder.TakenViaSupplier?.PartyName ? ` ${purchaseOrder.TakenViaSupplier?.PartyName}` : '');
}

function displayNameForPurchaseOrderItem(purchaseOrderItem: PurchaseOrderItem): string {
  const purchaseOrder = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem;
  return inlineLists`${[purchaseOrder?.OrderNumber, purchaseOrder?.TakenViaSupplier?.PartyName, purchaseOrderItem.Part?.Name].filter((v) => v)}`;
}

function displayNameForSerialisedItem(serialisedItem: SerialisedItem): string {
  return serialisedItem.ItemNumber + ' ' + serialisedItem.Name + ' SN: ' + serialisedItem.SerialNumber;
}

function displayNameForSerialisedItemCharacteristicType(serialisedItemCharacteristicType: SerialisedItemCharacteristicType): string {
  return (serialisedItemCharacteristicType.UnitOfMeasure ? serialisedItemCharacteristicType.Name + ' (' + serialisedItemCharacteristicType.UnitOfMeasure.Abbreviation + ')' : serialisedItemCharacteristicType.Name) ?? '';
}

function displayNameForTelecommunicationsNumber(telecommunicationsNumber: TelecommunicationsNumber): string {
  return inlineLists`${[telecommunicationsNumber.CountryCode, telecommunicationsNumber.AreaCode, telecommunicationsNumber.ContactNumber].filter((v) => v)}`;
}

function displayNameForUnifiedGood(unifiedGood: UnifiedGood): string {
  return unifiedGood.ProductCategoriesWhereProduct.map((v) => displayName(v)).join(', ');
}

function displayNameForWebAddress(webAddress: WebAddress): string {
  return webAddress.ElectronicAddressString ?? 'N/A';
}

function displayNameForWorkEffortPartyAssignment(workEffortPartyAssignment: WorkEffortPartyAssignment): string {
  return displayName(workEffortPartyAssignment.Party) ?? 'N/A';
}
