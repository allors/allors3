import { AssociationType, Composite, PropertyType, RoleType } from '@allors/workspace/meta/system';
import { Node } from '../../data/node';
import { IAssociationPattern } from './iassociation-pattern';
import { IRolePattern } from './irole-pattern';

export interface IPatternBase {
  objectType?: Composite;

  tree?: Node[];

  ofType?: Composite;
}

export type IPattern = IRolePattern | IAssociationPattern;

export type IPatternKind = IPattern['kind'];

export function pattern<T extends Composite>(objectType: T, propertyTypeFn: (objectType: T) => PropertyType, tree?: Node[], ofType?: Composite): IPattern {
  const propertyType = propertyTypeFn(objectType);
  if (propertyType.isAssociationType) {
    return {
      kind: 'AssociationPattern',
      objectType,
      associationType: propertyType as AssociationType,
      tree,
      ofType,
    };
  } else {
    return {
      kind: 'RolePattern',
      objectType,
      roleType: propertyType as RoleType,
      tree,
      ofType,
    };
  }
}
