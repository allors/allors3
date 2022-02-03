import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import { RoleType } from '@allors/system/workspace/meta';
import { CreatePullHandler } from './create-pull-handler';

export class CreateRolePullHandler implements CreatePullHandler {
  private static counter = 0;

  private name: string;

  constructor(
    public readonly roleType: RoleType,
    public readonly objectId: number
  ) {
    this.name = `CreateRole-${++CreateRolePullHandler.counter}`;
  }

  onPreCreatePull(pulls: Pull[]) {
    pulls.push({
      objectId: this.objectId,
      results: [
        {
          name: this.name,
        },
      ],
    });
  }

  onPostCreatePull(object: IObject, pullResult: IPullResult) {
    const role = pullResult.object(this.name);
    if (this.roleType.isOne) {
      object.strategy.setCompositeRole(this.roleType, role);
    } else {
      object.strategy.addCompositesRole(this.roleType, role);
    }
  }
}
