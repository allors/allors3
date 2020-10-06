export interface MetaData {
  interfaces?: ObjectTypeData[];
  classes?: ObjectTypeData[];
  relationTypes?: RelationTypeData[];
  methodTypes?: MethodTypeData[];
}

export interface ObjectTypeData {
  id: string;
  origin: string;
  name: string;
  plural: string;
  interfaceIds?: string[];
}

export interface RelationTypeData {
  id: string;
  origin: string;
  associationType: AssociationTypeData;
  roleType: RoleTypeData;
  concreteRoleTypes?: ConcreteRoleTypeData[];
  isDerived?: boolean;
}

export interface AssociationTypeData {
  objectTypeId: string;
  name: string;
  isOne: boolean;
}

export interface RoleTypeData {
  objectTypeId: string;
  singular: string;
  plural: string;
  isUnit: boolean;
  isOne: boolean;
  isRequired?: boolean;
  mediaType?: string;
}

export interface ConcreteRoleTypeData {
  objectTypeId: string;
  isRequired: boolean;
}

export interface MethodTypeData {
  id: string;
  objectTypeId: string;
  name: string;
}
