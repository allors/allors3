import { RoleType } from '@allors/system/workspace/meta';
import { IPullResult } from '../api/pull/ipull-result';
import { Pull } from '../api/pull/pull';
import { IObject } from '../iobject';
import { OnCreate } from './oncreate';

export class OnCreateRole implements OnCreate {
  private static counter = 0;

  private name: string;

  constructor(
    public readonly roleType: RoleType,
    public readonly objectId: number
  ) {
    this.name = `OnCreateRole-${++OnCreateRole.counter}`;
  }

  onPreCreate(pulls: Pull[]) {
    pulls.push({
      objectId: this.objectId,
      results: [
        {
          name: this.name,
        },
      ],
    });
  }

  onPostCreate(object: IObject, pullResult: IPullResult) {
    const role = pullResult.object(this.name);
    if (this.roleType.isOne) {
      object.strategy.setCompositeRole(this.roleType, role);
    } else {
      object.strategy.addCompositesRole(this.roleType, role);
    }
  }
}
