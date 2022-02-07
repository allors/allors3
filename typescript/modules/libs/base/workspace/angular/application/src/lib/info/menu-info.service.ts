import { Injectable } from '@angular/core';
import { MenuItem } from '../menu/menu-item';
import { MenuService } from '../..';

export interface MenuInfo {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuInfo[];
}

@Injectable()
export class MenuInfoService {
  constructor(private menuService: MenuService) {}

  write(allors: { [key: string]: unknown }) {
    allors['menu'] = this.menu;
  }

  private get menu(): string {
    const menuMapper = (v: MenuItem) => {
      return {
        tag: v.objectType?.tag,
        link: v.link,
        title: v.title,
        icon: v.icon,
        children: v.children?.map(menuMapper),
      };
    };

    const menu: MenuInfo[] = this.menuService.menu().map(menuMapper);

    return JSON.stringify(menu);
  }
}
