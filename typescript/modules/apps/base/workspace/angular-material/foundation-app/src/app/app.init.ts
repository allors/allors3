import { HttpClient } from '@angular/common/http';
import { WorkspaceService } from '@allors/workspace/angular/core';

import { AngularClient } from '../allors/angular-client';
import { configure } from './app.configure';
import { BaseContext } from '../allors/base-context';
import { Configuration } from '@allors/workspace/domain/system';
import { applyRules } from '@allors/workspace/derivations/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { ruleBuilder } from '@allors/workspace/derivations/base-custom';
import { DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { M } from '@allors/workspace/meta/default';

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
