import { WorkspaceService } from '@allors/workspace/angular/core';
import { Inject, Injectable } from '@angular/core';
import { MetaPopulation } from '@allors/workspace/meta/system';

import { angularList } from '../meta/angular.list';
import { angularOverview } from '../meta/angular.overview';
import { angularMenu } from '../meta/angular.menu';
import { MenuItem } from '../menu/menu-item';

import { MetaInfo, MenuInfo, DialogInfo } from './info';
import { OBJECT_CREATE_TOKEN, OBJECT_EDIT_TOKEN } from '../material/services/object/object.tokens';

@Injectable()
export class ReflectionService {
  metaPopulation: MetaPopulation;

  constructor(private workspaceService: WorkspaceService, @Inject(OBJECT_CREATE_TOKEN) private create: { [id: string]: any }, @Inject(OBJECT_EDIT_TOKEN) private edit: { [id: string]: any }) {
    this.metaPopulation = this.workspaceService.workspace.configuration.metaPopulation;
  }

  get meta(): string {
    const meta: MetaInfo[] = [...this.metaPopulation.composites].map((v) => {
      return {
        tag: v.tag,
        list: angularList(v),
        overview: angularOverview(v),
      };
    });

    return JSON.stringify(meta);
  }

  get dialogs(): string {
    const dialog: DialogInfo = {
      create: Object.keys(this.create).map((v) => ({ tag: v, component: this.create[v].name })),
      edit: Object.keys(this.edit).map((v) => ({ tag: v, component: this.edit[v].name })),
    };

    return JSON.stringify(dialog);
  }

  get menu(): string {
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
