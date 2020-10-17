import { RelationType } from './RelationType';
import { MetaPopulation } from './MetaPopulation';
import { ObjectType } from './ObjectType';
import { AssociationTypeData } from './Data';
import { RoleType } from './RoleType';
import { PropertyType } from './PropertyType';

export class AssociationType implements PropertyType {
  metaPopulation: MetaPopulation;
  roleType: RoleType;
  objectType: ObjectType;
  name: string;
  isOne: boolean;

  constructor(public relationType: RelationType, dataAssociationType: AssociationTypeData) {
    relationType.associationType = this;
    this.metaPopulation = relationType.metaPopulation;

    this.objectType = this.metaPopulation.metaObjectById.get(
      dataAssociationType.objectTypeId
    ) as ObjectType;
    this.name = dataAssociationType.name;
    this.isOne = dataAssociationType.isOne;
  }

  get isMany(): boolean {
    return !this.isOne;
  }
}
