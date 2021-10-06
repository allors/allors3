import { Procedure } from '../../data/procedure';
import { Pull } from '../../data/pull';

export interface PullRequest {
  /** Dependency Id */
  d?: string;

  /** List of Pulls */
  l?: Pull[];

  /** Procedure */
  p?: Procedure;
}
