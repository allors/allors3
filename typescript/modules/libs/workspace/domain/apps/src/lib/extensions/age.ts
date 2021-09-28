import { SerialisedItem } from '../generated';

export function age(serialisedItem: SerialisedItem): number {
  if (serialisedItem.canReadPurchasePrice && serialisedItem.ManufacturingYear != null) {
    return new Date().getFullYear() - serialisedItem.ManufacturingYear;
  } else {
    return 0;
  }
}
