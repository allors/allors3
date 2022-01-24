import { MetaPopulation } from '@allors/system/workspace/meta';
import { MenuItem } from '../menu/menu-item';

interface AngularMenuExtension {
  menu?: MenuItem[];
}

export function angularMenu(metaPopulation: MetaPopulation): MenuItem[];
export function angularMenu(
  metaPopulation: MetaPopulation,
  menu: MenuItem[]
): void;
export function angularMenu(
  metaPopulation: MetaPopulation,
  menu?: MenuItem[]
): MenuItem[] | void {
  if (metaPopulation == null) {
    return;
  }

  if (menu == null) {
    return (metaPopulation._ as AngularMenuExtension).menu;
  }

  (metaPopulation._ as AngularMenuExtension).menu = menu;
}
