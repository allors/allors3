import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortInventoryAssignment } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class WorkEffortInventoryAssignmentTotalSellingPriceRule implements IRule {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.patterns = [p(m.WorkEffortInventoryAssignment, (v) => v.DerivedBillableQuantity), p(m.WorkEffortInventoryAssignment, (v) => v.Quantity), p(m.WorkEffortInventoryAssignment, (v) => v.UnitSellingPrice)];
  }

  derive(cycle: ICycle, matches: WorkEffortInventoryAssignment[]) {
    for (const match of matches) {
      if (match.canReadUnitSellingPrice) {
        const quantity = match.DerivedBillableQuantity ? match.DerivedBillableQuantity : match.Quantity ? match.Quantity : '0';
        const unitSellingPrice = match.UnitSellingPrice ? match.UnitSellingPrice : '0';
        match.TotalSellingPrice = (parseFloat(quantity) * parseFloat(unitSellingPrice)).toFixed(2);
      } else {
        match.TotalSellingPrice = '0';
      }
    }
  }
}
