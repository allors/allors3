import { IDerivationService, IRule, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { DerivationService, Engine } from '@allors/workspace/configuration/core';
import { SessionServices } from './SessionServices';

export class WorkspaceServices implements IWorkspaceServices {
  workspace: IWorkspace;
  derivationService: IDerivationService;

  constructor(private rules: IRule[]) {}

  onInit(workspace: IWorkspace): void {
    this.workspace = workspace;

    const rules = this.rules;
    const engine = new Engine(rules);
    this.derivationService = new DerivationService(engine);
  }

  createSessionServices() {
    return new SessionServices();
  }
}
