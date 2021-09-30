import { ICycle, IRule, IPattern } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortInventoryAssignment } from '@allors/workspace/domain/default';

export class WorkEffortInventoryAssignmentTotalSellingPriceRule implements IRule {
  id: '4b0eb09beca54dd0b934cbeeea111e58';
  patterns: IPattern[];

  constructor(m: M) {
    const { treeBuilder: t } = m;

    this.patterns = [
      {
        kind: 'RolePattern',
        roleType: m.WorkEffortInventoryAssignment.DerivedBillableQuantity,
      },
      {
        kind: 'RolePattern',
        roleType: m.WorkEffortInventoryAssignment.Quantity,
      },
      {
        kind: 'RolePattern',
        roleType: m.WorkEffortInventoryAssignment.UnitSellingPrice,
      },
    ];
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
