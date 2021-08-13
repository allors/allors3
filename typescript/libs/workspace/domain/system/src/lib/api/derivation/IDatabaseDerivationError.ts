import { Role } from "../../Role";

export interface IDatabaseDerivationError {
  message: string;

  roles: Role[];
}
