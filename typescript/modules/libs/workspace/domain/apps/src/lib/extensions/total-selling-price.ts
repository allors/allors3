import { WorkEffortInventoryAssignment } from '../generated';

export function totalSellingPrice(workEffortInventoryAssignment: WorkEffortInventoryAssignment): string {
  if (workEffortInventoryAssignment.canReadUnitSellingPrice) {
    const quantity = workEffortInventoryAssignment.DerivedBillableQuantity ? workEffortInventoryAssignment.DerivedBillableQuantity : workEffortInventoryAssignment.Quantity ? workEffortInventoryAssignment.Quantity : '0';
    const unitSellingPrice = workEffortInventoryAssignment.UnitSellingPrice ? workEffortInventoryAssignment.UnitSellingPrice : '0';
    return (parseFloat(quantity) * parseFloat(unitSellingPrice)).toFixed(2);
  } else {
    return '0';
  }
}
