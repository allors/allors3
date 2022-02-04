import { HttpClient } from '@angular/common/http';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

import { AppClient } from './app.client';
import { Configuration } from '@allors/system/workspace/domain';
import { applyRules } from '@allors/system/workspace/derivations';
import { ruleBuilder } from '@allors/base/workspace/derivations-custom';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { AppContext } from './app.context';
import { configMenu } from './config/menu.config';
import { configNav } from './config/nav.config';

export function config(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  baseUrl: string,
  authUrl: string
) {
  const angularClient = new AppClient(httpClient, baseUrl, authUrl);

  const metaPopulation = new LazyMetaPopulation(data);
  const m = metaPopulation as unknown as M;

  let nextId = -1;

  const configuration: Configuration = {
    name: 'Default',
    metaPopulation,
    objectFactory: new PrototypeObjectFactory(metaPopulation),
    idGenerator: () => nextId--,
  };

  const rules = ruleBuilder(m);
  applyRules(m, rules);

  const database = new DatabaseConnection(configuration, angularClient);
  const workspace = database.createWorkspace();
  workspaceService.workspace = workspace;

  workspaceService.contextBuilder = () => new AppContext(workspaceService);

  configMenu(m);
  configNav(m);
}
