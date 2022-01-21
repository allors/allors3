import { Composite } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

export interface EditDialogData {
  readonly kind: 'EditDialogData';
  object: IObject;
  objectType?: Composite;
}
