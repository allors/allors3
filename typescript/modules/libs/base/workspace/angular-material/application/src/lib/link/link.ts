import { IObject } from '@allors/system/workspace/domain';
import { LinkType } from './link-type';

export interface Link {
  linkType: LinkType;
  target: IObject;
  icon: string;
  name: string;
  description: string;
}
