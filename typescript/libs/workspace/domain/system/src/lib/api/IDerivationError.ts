import { Role } from "../runtime/Role";

export interface IDerivationError {
  message: string;

  roles: Role[];
}
