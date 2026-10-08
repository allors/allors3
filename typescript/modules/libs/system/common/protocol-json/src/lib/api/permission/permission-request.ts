import { Request } from '../request';

export interface PermissionRequest extends Request {
  /** Permissions */
  p?: number[];
}
