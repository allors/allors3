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
  const extension = metaPopulation?._ as AngularMenuExtension;

  if (menu == null) {
    return extension?.menu;
  }

  extension.menu = menu;
}
