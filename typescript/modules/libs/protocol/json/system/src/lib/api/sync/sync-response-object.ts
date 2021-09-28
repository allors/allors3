import { SyncResponseRole } from './sync-response-role';

export interface SyncResponseObject {
  /** Id */
  i: number;

  /** Version */
  v: number;

  /** Class Tag */
  c: string;

  /** Sorted Grants */
  g: number[];

  /** Sorted Revocations */
  r: number[];

  /** Roles */
  ro: SyncResponseRole[];
}
