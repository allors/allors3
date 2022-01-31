import { AssociationType } from '@allors/system/workspace/meta';
import { IPullResult } from '../api/pull/ipull-result';
import { Pull } from '../api/pull/pull';
import { IObject } from '../iobject';
import { OnCreate } from './oncreate';

export class OnOCreateAssociation implements OnCreate {
  private static counter = 0;

  private name: string;

  constructor(
    public readonly associationType: AssociationType,
    public readonly objectId: number
  ) {
    this.name = `OnCreateAssociation-${++OnOCreateAssociation.counter}`;
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
    const association = pullResult.object(this.name);

    const roleType = this.associationType.roleType;
    if (roleType.isOne) {
      association.strategy.setCompositeRole(roleType, object);
    } else {
      association.strategy.addCompositesRole(roleType, object);
    }
  }
}
