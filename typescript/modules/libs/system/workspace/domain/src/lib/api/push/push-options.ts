import { Pull } from '../pull/pull';

export interface PushOptions {
  /** Refresh affected records after the protected save; omitted means true. */
  readonly refresh?: boolean;
  readonly discover?: Pull | Pull[];
}
