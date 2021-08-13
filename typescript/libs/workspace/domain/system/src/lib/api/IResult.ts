import { IObject } from '../IObject';
import { IDatabaseDerivationError } from './derivation/IDatabaseDerivationError';

export interface IResult {
  errorMessage: string;

  versionErrors: IObject[];

  accessErrors: IObject[];

  missingErrors: IObject[];

  derivationErrors: IDatabaseDerivationError[];
}
