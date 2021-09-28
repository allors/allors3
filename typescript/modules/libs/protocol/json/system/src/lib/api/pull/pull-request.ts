import { Procedure } from '../../data/procedure';
import { Pull } from '../../data/pull';

export interface PullRequest {
  /** List of Pulls */
  l?: Pull[];

  /** Procedure */
  p?: Procedure;
}
