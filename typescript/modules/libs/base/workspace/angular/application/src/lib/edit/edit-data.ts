import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';

export interface EditData {
  readonly kind: 'EditDialogData';
  object: IObject;
  objectType?: Composite;
}
