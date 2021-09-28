import { IDerivationService, IRule, IWorkspace, IWorkspaceServices } from '@allors/workspace/domain/system';
import { DerivationService, Engine } from '@allors/workspace/configuration/core';
import { IAngularMetaService } from '@allors/workspace/angular/base';
import { AngularMetaService } from '@allors/workspace/configuration/base';

import { SessionServices } from './session-services';

export class WorkspaceServices implements IWorkspaceServices {
  workspace: IWorkspace;

  derivationService: IDerivationService;
  angularMetaService: IAngularMetaService;

  constructor(private rules: IRule[]) {}
 
  onInit(workspace: IWorkspace): void {
    this.workspace = workspace;

    const rules = this.rules;
    const engine = new Engine(rules);
    this.derivationService = new DerivationService(engine);
    this.angularMetaService = new AngularMetaService();
  }

  createSessionServices() {
    return new SessionServices();
  }
}
