import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IRule } from '@allors/workspace/domain/system';
import { M } from '@allors/workspace/meta/default';
import { WorkEffortPartyAssignment } from '@allors/workspace/domain/default';

export class WorkEffortPartyAssignmentDisplayNameRule implements IRule<WorkEffortPartyAssignment> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.WorkEffortPartyAssignment;
    this.roleType = m.WorkEffortPartyAssignment.DisplayName;

    this.dependencies = [d(m.WorkEffortPartyAssignment, (v) => v.Party)];
  }

  derive(workEffortPartyAssignment: WorkEffortPartyAssignment) {
    return workEffortPartyAssignment.Party.DisplayName ?? 'N/A';
  }
}
