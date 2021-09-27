import { ObjectType } from '@allors/workspace/meta/system';
import { Extent } from '../../data/extent';
import { Result } from '../../data/result';
import { IObject } from '../../iobject';
import { TypeForParameter } from '../../types';

export interface Pull {
  extentRef?: string;

  extent?: Extent;

  objectType?: ObjectType;

  object?: IObject;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: TypeForParameter };
}
