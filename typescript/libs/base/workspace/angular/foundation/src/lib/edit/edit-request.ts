import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';

export interface EditRequest {
  readonly kind: 'EditRequest';
  objectId: number;
  objectType?: Composite;
}
