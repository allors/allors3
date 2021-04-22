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

export interface ObjectTypeData {
  /**
   * Tag
   */
  t: number;

  /**
   * Singular Name
   */
  s: string;

  /**
   * Plural Name
   */
  p?: string;

  /**
   * Origin
   */
  o?: number;

  /**
   * Interfaces (tags)
   */
  i?: number[];

  /**
   * Relation Types
   */
  r?: [tag: number, roleObjectType: number, singularName?: string, multiplicityOrSizeOrScaleAndPrecision?: number, pluralName?: string, origin?: number, isDerived?: boolean, isRequired?: boolean, mediaType?: string][];

  /**
   * Method Types
   */
  m?: [tag: number, name: string][];
}
