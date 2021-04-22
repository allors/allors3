export type UnitRelationTypeData = [tag: number, roleObjectType: number, singularName?: string, SizeOrScaleAndPrecision?: number, origin?: number, pluralName?: string, isDerived?: boolean, isRequired?: boolean, mediaType?: string];

export type CompositeRelationTypeData = [tag: number, roleObjectType: number, singularName?: string, multiplicity?: number, origin?: number, pluralName?: string, isDerived?: boolean, isRequired?: boolean];

export type RelationTypeData = UnitRelationTypeData | CompositeRelationTypeData;

export type MethodTypeData = [tag: number, name: string];

export type InterfaceData = [tag: number, singularName: string, directSupertypes?: number[], relationTypes?: RelationTypeData[], methodTypes?: MethodTypeData[], pluralName?: string];

export type ClassData = [tag: number, singularName: string, relationTypes?: RelationTypeData[], methodTypes?: MethodTypeData[], pluralName?: string];

export interface MetaData {
  /**
   * Interfaces
   */
  i?: InterfaceData[];

  /**
   * Classes
   */
  c?: ClassData[];
}
