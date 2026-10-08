import { Composite } from '@allors/system/workspace/meta';
import { IIdentifiable, TypeForParameter } from '../types';
import { Extent } from './extent';
import { Result } from './result';

export interface Pull {
  extentRef?: string;

  extent?: Extent;

  objectType?: Composite;

  object?: IIdentifiable;

  objectId?: number;

  results?: Result[];

  arguments?: { [name: string]: TypeForParameter };
}
