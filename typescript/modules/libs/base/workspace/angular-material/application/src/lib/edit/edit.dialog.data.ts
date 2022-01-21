import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/workspace/domain/system';

export interface EditDialogData {
  readonly kind: 'EditDialogData';
  object: IObject;
  objectType?: Composite;
}
