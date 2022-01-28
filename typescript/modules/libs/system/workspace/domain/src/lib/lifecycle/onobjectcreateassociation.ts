import { AssociationType } from '@allors/system/workspace/meta';
import { IPullResult } from '../api/pull/ipull-result';
import { Pull } from '../api/pull/pull';
import { IObject } from '../iobject';
import { OnObjectCreate } from './onobjectcreate';

export class OnObjectCreateAssociation implements OnObjectCreate {
  private static counter = 0;

  private name: string;

  constructor(
    public readonly associationType: AssociationType,
    public readonly objectId: number
  ) {
    this.name = `OnObjectCreateAssociation-${++OnObjectCreateAssociation.counter}`;
  }

  onObjectPreCreate(pulls: Pull[]) {
    pulls.push({
      objectId: this.objectId,
      results: [
        {
          name: this.name,
        },
      ],
    });
  }

  onObjectPostCreate(object: IObject, pullResult: IPullResult) {
    const association = pullResult.object(this.name);

    const roleType = this.associationType.roleType;
    if (roleType.isOne) {
      association.strategy.setCompositeRole(roleType, object);
    } else {
      association.strategy.addCompositesRole(roleType, object);
    }
  }
}
