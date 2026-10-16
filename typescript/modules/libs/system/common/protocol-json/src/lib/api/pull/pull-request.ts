import { Pull } from '../../data/pull';
import { Request } from '../request';
import { PullDependency } from './pull-dependency';

export interface PullRequest extends Request {
  /** Dependencies */
  d?: PullDependency[];

  /** List of Pulls */
  l?: Pull[];
}
