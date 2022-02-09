import { Class } from '@allors/system/workspace/meta';
import {
  CreatePullHandler,
  Initializer,
} from '@allors/system/workspace/domain';

export interface CreateRequest {
  readonly kind: 'CreateRequest';
  objectType: Class;
  initializer?: Initializer;
}
