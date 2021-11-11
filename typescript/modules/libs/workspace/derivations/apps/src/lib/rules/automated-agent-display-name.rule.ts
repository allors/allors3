import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { M } from '@allors/workspace/meta/default';
import { AutomatedAgent } from '@allors/workspace/domain/default';
import { IRule } from '@allors/workspace/domain/system';

export class AutomatedAgentDisplayNameRule implements IRule<AutomatedAgent> {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.roleType = m.AutomatedAgent.DisplayName;
  }

  derive(automatedAgent: AutomatedAgent) {
    automatedAgent.DisplayName = automatedAgent.UserName ?? 'N/A';
  }
}
