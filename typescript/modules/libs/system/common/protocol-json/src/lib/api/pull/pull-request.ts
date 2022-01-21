import { Procedure } from '../../data/procedure';
import { Pull } from '../../data/pull';
import { Request } from '../request';
import { PullDependency } from './pull-dependency';

export interface PullRequest extends Request {
  /** Dependencies */
  d?: PullDependency[];

  /** List of Pulls */
  l?: Pull[];

  /** Procedure */
  p?: Procedure;
}
