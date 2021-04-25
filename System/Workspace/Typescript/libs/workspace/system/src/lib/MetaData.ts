export type RelationTypeData =
  | [tag: number, roleObjectType: number, sizeOrScale?: number, precision?: number]
  | [tag: number, roleObjectType: number, singularName?: string, sizeOrScale?: number, precision?: number]
  | [tag: number, roleObjectType: number, singularName?: string, pluralName?: string, sizeOrScale?: number, precision?: number];

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

  /**
   * Multiplicity OneToOne
   */
  m0?: number[];

  /**
   * Multiplicity OneToMany
   */
  m1?: number[];

  /**
   * Multiplicity ManyToMany
   */
  m3?: number[];

  /**
   * Origin Workspace
   */
  o2?: number[];

  /**
   * Origin Session
   */
  o4?: number[];

  /**
   * IsDerived
   */
  d?: number[];

  /**
   * IsRequired
   */
  r?: number[];

  /**
   * IsUniqe
   */
  u?: number[];

  /**
   * MediaType
   */
  m?: { [name: string]: string };
}
