import { Injectable } from '@angular/core';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { angularMenu } from '../meta/angular.menu';
import { MenuItem } from '../menu/menu-item';
import { WorkspaceService } from '../workspace/workspace-service';

export interface MenuInfo {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuInfo[];
}

@Injectable()
export class MenuInfoService {
  metaPopulation: MetaPopulation;

  constructor(private workspaceService: WorkspaceService) {
    this.metaPopulation =
      this.workspaceService.workspace.configuration.metaPopulation;
  }

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

    const menu: MenuInfo[] = angularMenu(this.metaPopulation).map(menuMapper);

    return JSON.stringify(menu);
  }
}
