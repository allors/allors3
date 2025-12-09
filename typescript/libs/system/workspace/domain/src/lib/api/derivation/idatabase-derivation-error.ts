import { Role } from '../../role';

export interface IDatabaseDerivationError {
  message: string;

  roles: Role[];
}
