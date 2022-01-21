import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortInventoryAssignment } from '@allors/workspace/domain/default';

export class WorkEffortInventoryAssignmentTotalSellingPriceRule
  implements IRule<WorkEffortInventoryAssignment>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.WorkEffortInventoryAssignment;
    this.roleType = m.WorkEffortInventoryAssignment.TotalSellingPrice;
  }

  derive(workEffortInventoryAssignment: WorkEffortInventoryAssignment) {
    if (workEffortInventoryAssignment.canReadUnitSellingPrice) {
      const quantity = workEffortInventoryAssignment.DerivedBillableQuantity
        ? workEffortInventoryAssignment.DerivedBillableQuantity
        : workEffortInventoryAssignment.Quantity
        ? workEffortInventoryAssignment.Quantity
        : '0';
      const unitSellingPrice = workEffortInventoryAssignment.UnitSellingPrice
        ? workEffortInventoryAssignment.UnitSellingPrice
        : '0';
      return (parseFloat(quantity) * parseFloat(unitSellingPrice)).toFixed(2);
    }

    return '0';
  }
}
