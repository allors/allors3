import { ObjectType } from '@allors/workspace/meta/system';
import { Extent } from '../../data/Extent';
import { Result } from '../../data/Result';
import { IObject } from '../../runtime/IObject';
import { TypeForParameter } from '../../runtime/Types';

export interface Pull {
  extentRef?: string;

  extent?: Extent;

  objectType?: ObjectType;

  object?: IObject;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: TypeForParameter };
}
