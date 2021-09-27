import * as fs from 'fs';

import { data } from '@allors/workspace/meta/json/default';
import { menu } from './app/main/main.menu';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { AngularMetaService } from '@allors/workspace/configuration/base';
import { configure } from './app/configure';
import { M } from '@allors/workspace/meta/default';

interface MetaInfo {
  tag: string;
  list: string;
  overview: string;
}

describe('Scaffold', () => {
  it('meta and menu', () => {
    const metaPopulation = new LazyMetaPopulation(data);
    const angularMetaService = new AngularMetaService();

    const m: M = metaPopulation as unknown as M;

    configure(m, angularMetaService);

    const meta: MetaInfo[] = [...metaPopulation.composites].map((v) => {
      const angularMeta = angularMetaService.for(v);

      return {
        tag: v.tag,
        list: angularMeta.list,
        overview: angularMeta.overview,
      };
    });

    fs.mkdirSync('./dist/scaffold', { recursive: true } as any);
    fs.writeFileSync('./dist/scaffold/meta.json', JSON.stringify(meta));
    fs.writeFileSync('./dist/scaffold/menu.json', JSON.stringify(menu));
  });
});
