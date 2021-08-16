import { IObject } from '../IObject';
import { IDatabaseDerivationError } from './derivation/IDatabaseDerivationError';

export interface IResult {

  hasErrors: boolean;
  
  errorMessage: string;

  versionErrors: IObject[];

  accessErrors: IObject[];

  missingErrors: IObject[];

  derivationErrors: IDatabaseDerivationError[];
}
