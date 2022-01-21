import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { WorkspaceService } from '@allors/workspace/angular/core';
import {
  AllorsMaterialSideNavService,
  angularList,
  angularMenu,
  angularOverview,
  MenuItem,
} from '@allors/workspace/angular/base';
import { M } from '@allors/default/workspace/meta';
import { create, edit } from '../app/app.module';

interface AllorsInfo {
  meta: MetaInfo[];
  menu: MenuInfo[];
  dialog: DialogInfo;
}

interface MetaInfo {
  tag: string;
  list: string;
  overview: string;
}

interface MenuInfo {
  tag?: string;
  link?: string;
  title?: string;
  icon?: string;
  children?: MenuInfo[];
}

interface DialogObjectInfo {
  tag?: string;
  component?: string;
}

interface DialogInfo {
  create?: DialogObjectInfo[];
  edit?: DialogObjectInfo[];
}

@Component({
  template: `<h1>Allors</h1>
    <div>Info: {{ allors != null }}</div>`,
})
export class AllorsComponent implements OnInit {
  constructor(
    private workspaceService: WorkspaceService,
    private router: Router,
    private sideNavService: AllorsMaterialSideNavService
  ) {}

  allors: AllorsInfo;

  public ngOnInit(): void {
    const metaPopulation =
      this.workspaceService.workspace.configuration.metaPopulation;
    const m = metaPopulation as M;

    const meta: MetaInfo[] = [...metaPopulation.composites].map((v) => {
      return {
        tag: v.tag,
        list: angularList(v),
        overview: angularOverview(v),
      };
    });

    const menuMapper = (v: MenuItem) => {
      return {
        tag: v.objectType?.tag,
        link: v.link,
        title: v.title,
        icon: v.icon,
        children: v.children?.map(menuMapper),
      };
    };

    const menu: MenuInfo[] = angularMenu(m).map(menuMapper);

    const dialog: DialogInfo = {
      create: Object.keys(create).map((v) => ({
        tag: v,
        component: create[v].name,
      })),
      edit: Object.keys(edit).map((v) => ({ tag: v, component: edit[v].name })),
    };

    this.allors = {
      meta,
      menu,
      dialog,
    };

    window['allors'] = JSON.stringify(this.allors);
  }
}
