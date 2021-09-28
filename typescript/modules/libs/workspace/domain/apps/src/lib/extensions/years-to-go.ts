import { SerialisedItem, UnifiedGood } from '../generated';
import { age } from './age';

export function yearsToGo(serialisedItem: SerialisedItem): number {
  const good = serialisedItem.PartWhereSerialisedItem as UnifiedGood | null;

  if (serialisedItem.canReadPurchasePrice && serialisedItem.ManufacturingYear != null && good?.LifeTime != null) {
    return good.LifeTime - age(serialisedItem) < 0 ? 0 : good.LifeTime - age(serialisedItem);
  } else {
    return 0;
  }
}
