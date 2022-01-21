import * as fs from 'fs';

import { data } from '@allors/workspace/meta/json/default';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { configure } from './app/configure';
import { M } from '@allors/default/workspace/meta';
import {
  angularList,
  angularMenu,
  angularOverview,
  MenuItem,
} from '@allors/workspace/angular/base';

// import { create, edit } from './app/app.module';

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

describe('Scaffold', () => {
  it('meta and menu', () => {
    const metaPopulation = new LazyMetaPopulation(data);
    const m: M = metaPopulation as unknown as M;

    configure(m);

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

    // const dialogs = {
    //   create: Object.keys(create).map((v) => ({ tag: v, component: create[v].name })),
    //   edit: Object.keys(edit).map((v) => ({ tag: v, component: edit[v].name })),
    // };

    fs.mkdirSync('./dist/base', { recursive: true } as any);
    fs.writeFileSync('./dist/base/meta.json', JSON.stringify(meta));
    fs.writeFileSync('./dist/base/menu.json', JSON.stringify(menu));
    // fs.writeFileSync('./dist/base/dialogs.json', JSON.stringify(dialogs));
  });
});
