import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { M } from '@allors/workspace/meta/default';
import { IRule } from '@allors/workspace/domain/system';
import { AutomatedAgent } from '@allors/workspace/domain/default';

export class AutomatedAgentDisplayNameRule implements IRule<AutomatedAgent> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.AutomatedAgent;
    this.roleType = m.AutomatedAgent.DisplayName;
  }

  derive(automatedAgent: AutomatedAgent) {
    return automatedAgent.UserName ?? 'N/A';
  }
}
