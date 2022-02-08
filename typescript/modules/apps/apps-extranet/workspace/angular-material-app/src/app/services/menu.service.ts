import {
  MenuItem,
  MenuService,
} from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMenuService implements MenuService {
  private _menu: MenuItem[];

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    this._menu = [
      { title: 'Home', icon: 'home', link: '/' },
      {
        title: 'Contacts',
        icon: 'business',
        children: [
          { objectType: m.Person },
          { objectType: m.Organisation },
          { objectType: m.Country },
        ],
      },
      { title: 'Fields', icon: 'build', link: '/fields' },
    ];
  }

  menu(): MenuItem[] {
    return this._menu;
  }
}
