import { Role } from "../runtime/operands/Role";

export interface IDerivationError {
  message: string;

  roles: Role[];
}
