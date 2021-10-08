import { Procedure } from '../../data/procedure';
import { Pull } from '../../data/pull';
import { PullDependency } from './pull-dependency';

export interface PullRequest {
  /** Dependencies */
  d?: PullDependency[];

  /** List of Pulls */
  l?: Pull[];

  /** Procedure */
  p?: Procedure;
}
