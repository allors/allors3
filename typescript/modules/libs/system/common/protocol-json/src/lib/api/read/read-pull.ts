import { Pull } from '../../data/pull';
import { PullDependency } from '../pull/pull-dependency';

export interface ReadPull {
  /** Declarative selections. */
  l: Pull[];

  /** Configured dependencies. */
  d: PullDependency[];
}
