import { AssociationDependency, AssociationType, Composite } from '@allors/workspace/meta/system';

export class LazyAssociationDependency implements AssociationDependency {
  readonly kind = 'AssociationDependency';

  constructor(public readonly objectType: Composite, public readonly associationType: AssociationType) {}
}
