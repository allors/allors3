import { InventoryItem } from '../generated';

export function facilityName(inventoryItem: InventoryItem): string {
  return inventoryItem.Facility?.Name ?? '';
}
