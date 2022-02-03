import { AssociationType } from '@allors/system/workspace/meta';
import { IObject, IPullResult, Pull } from '@allors/system/workspace/domain';
import { CreatePullHandler } from './create-pull-handler';

export class CreateAssociationPullHandler implements CreatePullHandler {
  private static counter = 0;

  private name: string;

  constructor(
    public readonly associationType: AssociationType,
    public readonly objectId: number
  ) {
    this.name = `CreateAssociation-${++CreateAssociationPullHandler.counter}`;
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
    const association = pullResult.object(this.name);

    const roleType = this.associationType.roleType;
    if (roleType.isOne) {
      association.strategy.setCompositeRole(roleType, object);
    } else {
      association.strategy.addCompositesRole(roleType, object);
    }
  }
}
