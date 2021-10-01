import { IDerivationService, IRule, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { DerivationService, Engine } from '@allors/workspace/configuration/core';

import { SessionServices } from './session-services';
import { M } from '@allors/workspace/meta/default';

export class WorkspaceServices implements IWorkspaceServices {
  workspace: IWorkspace;

  derivationService: IDerivationService;

  constructor(private rules: IRule[]) {}
  m: M;

  onInit(workspace: IWorkspace): void {
    this.workspace = workspace;

    this.m = workspace.configuration.metaPopulation as M;

    const rules = this.rules;
    const engine = new Engine(rules);
    this.derivationService = new DerivationService(engine);
  }

  createSessionServices() {
    return new SessionServices();
  }
}
