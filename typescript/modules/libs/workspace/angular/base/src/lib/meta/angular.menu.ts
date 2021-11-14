import { MetaPopulation } from '@allors/workspace/meta/system';
import { MenuItem } from '../menu/menu-item';

interface AngularMenuExtension {
  menu?: MenuItem[];
}

export function angularMenu(metaPopulation: MetaPopulation, menu?: MenuItem[]) {
  if (menu == null) {
    return (metaPopulation._ as AngularMenuExtension).menu;
  }

  (metaPopulation._ as AngularMenuExtension).menu = menu;
}
