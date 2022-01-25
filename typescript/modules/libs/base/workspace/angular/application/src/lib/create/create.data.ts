import { Composite } from '@allors/system/workspace/meta';
import { OnCreate } from '@allors/base/workspace/angular/application';

export interface CreateData {
  readonly kind: 'CreateData';
  objectType: Composite;
  onCreate?: OnCreate;
}
