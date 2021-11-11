import { ICycle, IRule, IPattern, pattern as p } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortPartyAssignment } from '@allors/workspace/domain/default';
import { Dependency } from '@allors/workspace/meta/system';

export class WorkEffortPartyAssignmentDisplayNameRule implements IRule {
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { treeBuilder: t, dependency: d } = m;

    this.patterns = [p(m.WorkEffortPartyAssignment, (v) => v.Party)];

    this.dependencies = [d(m.WorkEffortPartyAssignment, (v) => v.Party)];
  }

  derive(cycle: ICycle, matches: WorkEffortPartyAssignment[]) {
    for (const match of matches) {
      match.DisplayName = match.Party.DisplayName ?? 'N/A';
    }
  }
}
