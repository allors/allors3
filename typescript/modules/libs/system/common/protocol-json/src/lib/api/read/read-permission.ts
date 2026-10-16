import { ReadPermissionPayload } from './read-permission-payload';

export interface ReadPermission {
  /** Immutable permission ID within the population. */
  i: number;

  /** Full descriptor or exact request-permission reuse. */
  p: ReadPermissionPayload;
}
