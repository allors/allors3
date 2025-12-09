import { IObject } from '@allors/system/workspace/domain';
import { HyperlinkType } from './hyperlink-type';

export interface Hyperlink {
  linkType: HyperlinkType;
  target: IObject;
  icon: string;
  name: string;
  description: string;
}
