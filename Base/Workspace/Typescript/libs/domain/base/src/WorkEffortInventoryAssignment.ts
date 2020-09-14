import { Meta } from '@allors/meta/generated';
import { assert } from '@allors/meta/system';
import { WorkEffortInventoryAssignment } from '@allors/domain/generated';
import { Database } from '@allors/workspace/system';

export function extendWorkEffortInventoryAssignment(database: Database) {
  const m = database.metaPopulation as Meta;
  const cls = database.constructorByObjectType.get(m.WorkEffortInventoryAssignment);
  assert(cls);

  Object.defineProperty(cls.prototype, 'totalSellingPrice', {
    configurable: true,
    get(this: WorkEffortInventoryAssignment): string {
      if (this.CanReadUnitSellingPrice) {
        const quantity = this.BillableQuantity ? this.BillableQuantity : this.Quantity ? this.Quantity : '0';
        const unitSellingPrice = this.UnitSellingPrice ? this.UnitSellingPrice : '0';
        return (parseFloat(quantity) * parseFloat(unitSellingPrice)).toFixed(2);
      } else {
        return '0';
      }
    },
  });
}
