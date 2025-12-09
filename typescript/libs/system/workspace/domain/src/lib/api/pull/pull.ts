import { Composite } from '@allors/system/workspace/meta';
import { Extent } from '../../data/extent';
import { Result } from '../../data/result';
import { IObject } from '../../iobject';
import { TypeForParameter } from '../../types';

export interface Pull {
  extentRef?: string;

  extent?: Extent;

  objectType?: Composite;

  object?: IObject;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: TypeForParameter };
}
