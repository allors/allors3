import { HttpClient } from '@angular/common/http';
import { WorkspaceService } from '@allors/workspace/angular/core';

import { AngularClient } from '../allors/angular-client';
import { configure } from './app.configure';
import { BaseContext } from '../allors/base-context';
import { Configuration } from '@allors/system/workspace/domain';
import { applyRules } from '@allors/system/workspace/derivations';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { ruleBuilder } from '@allors/workspace/derivations/base-custom';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';

export function init(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  baseUrl: string,
  authUrl: string
) {
  const angularClient = new AngularClient(httpClient, baseUrl, authUrl);

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

  workspaceService.contextBuilder = () => new BaseContext(workspaceService);

  configure(m);
}
