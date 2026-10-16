import { DiscoverySuccess } from './discovery-success';
import { ReadFailure } from './read-failure';
import { ReadSuccess } from './read-success';

/** Wire alternatives only; decoded messages still require runtime validation. */
export type ReadResponse = DiscoverySuccess | ReadSuccess | ReadFailure;
