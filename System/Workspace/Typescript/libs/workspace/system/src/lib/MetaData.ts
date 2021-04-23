export type UnitRelationTypeData = [
  tag: number,
  roleObjectType: number,
  singularName?: string,
  SizeOrScaleAndPrecision?: number | string,
  origin?: number,
  pluralName?: string,
  isDerived?: boolean,
  isRequired?: boolean,
  isUnique?: boolean,
  mediaType?: string
];

export type CompositeRelationTypeData = [
  tag: number,
   roleObjectType: number,
    singularName?: string,
     multiplicity?: number,
      origin?: number,
       pluralName?: string,
        isDerived?: boolean,
         isRequired?: boolean,
          isUnique?: boolean];

export type RelationTypeData = UnitRelationTypeData | CompositeRelationTypeData;

export type MethodTypeData = [tag: number, name: string];

export type ObjectTypeData = [tag: number, singularName: string, directSupertypes?: number[], relationTypes?: RelationTypeData[], methodTypes?: MethodTypeData[], pluralName?: string];

export interface MetaData {
  /**
   * Interfaces
   */
  i?: ObjectTypeData[];

  /**
   * Classes
   */
  c?: ObjectTypeData[];
}
