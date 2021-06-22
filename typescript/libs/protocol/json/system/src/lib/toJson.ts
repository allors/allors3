import { Pull as DataPull, Pull } from '@allors/workspace/domain/system';
import { ToJsonVisitor } from "./ToJsonVisitor";
import { UnitConvert } from './UnitConvert';

const unitConvert = new UnitConvert();

export function toJson(pull: DataPull): Pull {
  const visitor = new ToJsonVisitor(unitConvert);
  visitor.acceptPull(pull);
  return visitor.Pull;
}
