import { ObjectType } from '@allors/workspace/meta/system';
import { InternalMetaObject } from './internal-meta-object';

export interface InternalObjectType extends InternalMetaObject, ObjectType {}
