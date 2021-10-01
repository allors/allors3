import { Composite } from '@allors/workspace/meta/system';

export interface MenuItem {
  objectType?: Composite;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuItem[];
}
