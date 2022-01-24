import { Composite } from '@allors/system/workspace/meta';

export interface MenuItem {
  objectType?: Composite;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuItem[];
}
