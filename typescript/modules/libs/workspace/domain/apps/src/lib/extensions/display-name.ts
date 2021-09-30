import { oneLine, stripIndents, inlineLists } from 'common-tags';
import {
  Organisation,
  Person,
  AutomatedAgent,
  EmailAddress,
  FixedAsset,
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
import { tags } from '@allors/workspace/meta/default';

export function displayName(
  item:
    | Party
    | EmailAddress
    | FixedAsset
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

  return undefined;
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
