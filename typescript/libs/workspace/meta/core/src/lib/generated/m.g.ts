/* Allors generated file. Do not edit, changes will be overwritten. */
/* eslint-disable @typescript-eslint/ban-types */
/* eslint-disable @typescript-eslint/no-empty-interface */
import { MetaPopulation, Unit, Interface, Class, AssociationType, RoleType, MethodType } from '@allors/workspace/system';

export interface M extends MetaPopulation {
  Binary: Binary;
  Boolean: Boolean;
  DateTime: DateTime;
  Decimal: Decimal;
  Float: Float;
  Integer: Integer;
  String: String;
  Unique: Unique;

  Deletable: Deletable;
  UniquelyIdentifiable: UniquelyIdentifiable;
  Permission: Permission;
  User: User;
  I1: I1;
  I12: I12;
  I2: I2;
  S1: S1;

  C1: C1;
  C2: C2;
  Data: Data;
  Organisation: Organisation;
  Person: Person;
  UnitSample: UnitSample;
  SessionOrganisation: SessionOrganisation;
  SessionPerson: SessionPerson;
  WorkspaceOrganisation: WorkspaceOrganisation;
  WorkspacePerson: WorkspacePerson;
}

export type Binary = Unit;
export type Boolean = Unit;
export type DateTime = Unit;
export type Decimal = Unit;
export type Float = Unit;
export type Integer = Unit;
export type String = Unit;
export type Unique = Unit;

export interface Deletable extends Interface {
  AsOrganisation: Organisation;
  AsPerson: Person;
  AsPermission: Permission;
  AsUser: User;

  Delete: MethodType;
}

export interface Object extends Interface {}

export interface UniquelyIdentifiable extends Interface {
  AsOrganisation: Organisation;
  AsPerson: Person;
  AsUser: User;

  UniqueId: UniquelyIdentifiableUniqueId;
}

export interface DelegatedAccessControlledObject extends Interface {}

export interface Permission extends Interface {
  Delete: MethodType;
}

export interface SecurityTokenOwner extends Interface {}

export interface User extends Interface {
  AsPerson: Person;

  Delete: MethodType;

  UniqueId: UniquelyIdentifiableUniqueId;
  UserName: UserUserName;
  InUserPassword: UserInUserPassword;
  UserEmail: UserUserEmail;
}

export interface DerivationCounted extends Interface {}

export interface ValidationI12 extends Interface {}

export interface I1 extends Interface {
  AsC1: C1;

  I1I1Many2One: I1I1I1Many2One;
  I1I12Many2Manies: I1I1I12Many2Manies;
  I1I2Many2Manies: I1I1I2Many2Manies;
  I1I2Many2One: I1I1I2Many2One;
  I1AllorsString: I1I1AllorsString;
  I1I12Many2One: I1I1I12Many2One;
  I1AllorsDateTime: I1I1AllorsDateTime;
  I1I2One2Manies: I1I1I2One2Manies;
  I1C2One2Manies: I1I1C2One2Manies;
  I1C1One2One: I1I1C1One2One;
  I1AllorsInteger: I1I1AllorsInteger;
  I1C2Many2Manies: I1I1C2Many2Manies;
  I1I1One2Manies: I1I1I1One2Manies;
  I1I1Many2Manies: I1I1I1Many2Manies;
  I1AllorsBoolean: I1I1AllorsBoolean;
  I1AllorsDecimal: I1I1AllorsDecimal;
  I1I12One2One: I1I1I12One2One;
  I1I2One2One: I1I1I2One2One;
  I1C2One2One: I1I1C2One2One;
  I1C1One2Manies: I1I1C1One2Manies;
  I1AllorsBinary: I1I1AllorsBinary;
  I1C1Many2Manies: I1I1C1Many2Manies;
  I1AllorsDouble: I1I1AllorsDouble;
  I1I1One2One: I1I1I1One2One;
  I1C1Many2One: I1I1C1Many2One;
  I1I12One2Manies: I1I1I12One2Manies;
  I1C2Many2One: I1I1C2Many2One;
  I1AllorsUnique: I1I1AllorsUnique;
  I12AllorsBinary: I12I12AllorsBinary;
  I12C2One2One: I12I12C2One2One;
  I12AllorsDouble: I12I12AllorsDouble;
  I12I1Many2One: I12I12I1Many2One;
  I12AllorsString: I12I12AllorsString;
  I12I12Many2Manies: I12I12I12Many2Manies;
  I12AllorsDecimal: I12I12AllorsDecimal;
  I12I2Many2Manies: I12I12I2Many2Manies;
  I12C2Many2Manies: I12I12C2Many2Manies;
  I12I1Many2Manies: I12I12I1Many2Manies;
  I12I12One2Manies: I12I12I12One2Manies;
  Name: I12Name;
  I12C1Many2Manies: I12I12C1Many2Manies;
  I12I2Many2One: I12I12I2Many2One;
  I12AllorsUnique: I12I12AllorsUnique;
  I12AllorsInteger: I12I12AllorsInteger;
  I12I1One2Manies: I12I12I1One2Manies;
  I12C1One2One: I12I12C1One2One;
  I12I12One2One: I12I12I12One2One;
  I12I2One2One: I12I12I2One2One;
  Dependencies: I12Dependencies;
  I12I2One2Manies: I12I12I2One2Manies;
  I12C2Many2One: I12I12C2Many2One;
  I12I12Many2One: I12I12I12Many2One;
  I12AllorsBoolean: I12I12AllorsBoolean;
  I12I1One2One: I12I12I1One2One;
  I12C1One2Manies: I12I12C1One2Manies;
  I12C1Many2One: I12I12C1Many2One;
  I12AllorsDateTime: I12I12AllorsDateTime;

  C1sWhereC1I12Many2Many: C1sWhereC1I12Many2Many;
  C1sWhereC1I12Many2One: C1sWhereC1I12Many2One;
  C1WhereC1I12One2Many: C1WhereC1I12One2Many;
  C1WhereC1I12One2One: C1WhereC1I12One2One;
  C1sWhereC1I1Many2Many: C1sWhereC1I1Many2Many;
  C1sWhereC1I1Many2One: C1sWhereC1I1Many2One;
  C1WhereC1I1One2Many: C1WhereC1I1One2Many;
  C1WhereC1I1One2One: C1WhereC1I1One2One;
  C2sWhereC2I12Many2One: C2sWhereC2I12Many2One;
  C2WhereC2I12One2One: C2WhereC2I12One2One;
  C2sWhereC2I1Many2Many: C2sWhereC2I1Many2Many;
  C2WhereC2I1One2Many: C2WhereC2I1One2Many;
  C2sWhereC2I12Many2Many: C2sWhereC2I12Many2Many;
  C2sWhereC2I1Many2One: C2sWhereC2I1Many2One;
  C2WhereC2I1One2One: C2WhereC2I1One2One;
  C2WhereC2I12One2Many: C2WhereC2I12One2Many;
  C2WhereS1One2One: C2WhereS1One2One;
  I1sWhereI1I1Many2One: I1sWhereI1I1Many2One;
  I1sWhereI1I12Many2Many: I1sWhereI1I12Many2Many;
  I1sWhereI1I12Many2One: I1sWhereI1I12Many2One;
  I1WhereI1I1One2Many: I1WhereI1I1One2Many;
  I1sWhereI1I1Many2Many: I1sWhereI1I1Many2Many;
  I1WhereI1I12One2One: I1WhereI1I12One2One;
  I1WhereI1I1One2One: I1WhereI1I1One2One;
  I1WhereI1I12One2Many: I1WhereI1I12One2Many;
  I12sWhereI12I1Many2One: I12sWhereI12I1Many2One;
  I12sWhereI12I12Many2Many: I12sWhereI12I12Many2Many;
  I12sWhereI12I1Many2Many: I12sWhereI12I1Many2Many;
  I12WhereI12I12One2Many: I12WhereI12I12One2Many;
  I12WhereI12I1One2Many: I12WhereI12I1One2Many;
  I12WhereI12I12One2One: I12WhereI12I12One2One;
  I12sWhereDependency: I12sWhereDependency;
  I12sWhereI12I12Many2One: I12sWhereI12I12Many2One;
  I12WhereI12I1One2One: I12WhereI12I1One2One;
  I2sWhereI2I12Many2One: I2sWhereI2I12Many2One;
  I2sWhereI2I1Many2One: I2sWhereI2I1Many2One;
  I2WhereI2I12One2Many: I2WhereI2I12One2Many;
  I2WhereI2I12One2One: I2WhereI2I12One2One;
  I2sWhereI2I1Many2Many: I2sWhereI2I1Many2Many;
  I2WhereI2I1One2One: I2WhereI2I1One2One;
  I2WhereI2I1One2Many: I2WhereI2I1One2Many;
  I2sWhereI2I12Many2Many: I2sWhereI2I12Many2Many;
}

export interface I12 extends Interface {
  AsC1: C1;
  AsC2: C2;
  AsI1: I1;
  AsI2: I2;

  I12AllorsBinary: I12I12AllorsBinary;
  I12C2One2One: I12I12C2One2One;
  I12AllorsDouble: I12I12AllorsDouble;
  I12I1Many2One: I12I12I1Many2One;
  I12AllorsString: I12I12AllorsString;
  I12I12Many2Manies: I12I12I12Many2Manies;
  I12AllorsDecimal: I12I12AllorsDecimal;
  I12I2Many2Manies: I12I12I2Many2Manies;
  I12C2Many2Manies: I12I12C2Many2Manies;
  I12I1Many2Manies: I12I12I1Many2Manies;
  I12I12One2Manies: I12I12I12One2Manies;
  Name: I12Name;
  I12C1Many2Manies: I12I12C1Many2Manies;
  I12I2Many2One: I12I12I2Many2One;
  I12AllorsUnique: I12I12AllorsUnique;
  I12AllorsInteger: I12I12AllorsInteger;
  I12I1One2Manies: I12I12I1One2Manies;
  I12C1One2One: I12I12C1One2One;
  I12I12One2One: I12I12I12One2One;
  I12I2One2One: I12I12I2One2One;
  Dependencies: I12Dependencies;
  I12I2One2Manies: I12I12I2One2Manies;
  I12C2Many2One: I12I12C2Many2One;
  I12I12Many2One: I12I12I12Many2One;
  I12AllorsBoolean: I12I12AllorsBoolean;
  I12I1One2One: I12I12I1One2One;
  I12C1One2Manies: I12I12C1One2Manies;
  I12C1Many2One: I12I12C1Many2One;
  I12AllorsDateTime: I12I12AllorsDateTime;

  C1sWhereC1I12Many2Many: C1sWhereC1I12Many2Many;
  C1sWhereC1I12Many2One: C1sWhereC1I12Many2One;
  C1WhereC1I12One2Many: C1WhereC1I12One2Many;
  C1WhereC1I12One2One: C1WhereC1I12One2One;
  C2sWhereC2I12Many2One: C2sWhereC2I12Many2One;
  C2WhereC2I12One2One: C2WhereC2I12One2One;
  C2sWhereC2I12Many2Many: C2sWhereC2I12Many2Many;
  C2WhereC2I12One2Many: C2WhereC2I12One2Many;
  I1sWhereI1I12Many2Many: I1sWhereI1I12Many2Many;
  I1sWhereI1I12Many2One: I1sWhereI1I12Many2One;
  I1WhereI1I12One2One: I1WhereI1I12One2One;
  I1WhereI1I12One2Many: I1WhereI1I12One2Many;
  I12sWhereI12I12Many2Many: I12sWhereI12I12Many2Many;
  I12WhereI12I12One2Many: I12WhereI12I12One2Many;
  I12WhereI12I12One2One: I12WhereI12I12One2One;
  I12sWhereDependency: I12sWhereDependency;
  I12sWhereI12I12Many2One: I12sWhereI12I12Many2One;
  I2sWhereI2I12Many2One: I2sWhereI2I12Many2One;
  I2WhereI2I12One2Many: I2WhereI2I12One2Many;
  I2WhereI2I12One2One: I2WhereI2I12One2One;
  I2sWhereI2I12Many2Many: I2sWhereI2I12Many2Many;
}

export interface I2 extends Interface {
  AsC2: C2;

  I12AllorsBinary: I12I12AllorsBinary;
  I12C2One2One: I12I12C2One2One;
  I12AllorsDouble: I12I12AllorsDouble;
  I12I1Many2One: I12I12I1Many2One;
  I12AllorsString: I12I12AllorsString;
  I12I12Many2Manies: I12I12I12Many2Manies;
  I12AllorsDecimal: I12I12AllorsDecimal;
  I12I2Many2Manies: I12I12I2Many2Manies;
  I12C2Many2Manies: I12I12C2Many2Manies;
  I12I1Many2Manies: I12I12I1Many2Manies;
  I12I12One2Manies: I12I12I12One2Manies;
  Name: I12Name;
  I12C1Many2Manies: I12I12C1Many2Manies;
  I12I2Many2One: I12I12I2Many2One;
  I12AllorsUnique: I12I12AllorsUnique;
  I12AllorsInteger: I12I12AllorsInteger;
  I12I1One2Manies: I12I12I1One2Manies;
  I12C1One2One: I12I12C1One2One;
  I12I12One2One: I12I12I12One2One;
  I12I2One2One: I12I12I2One2One;
  Dependencies: I12Dependencies;
  I12I2One2Manies: I12I12I2One2Manies;
  I12C2Many2One: I12I12C2Many2One;
  I12I12Many2One: I12I12I12Many2One;
  I12AllorsBoolean: I12I12AllorsBoolean;
  I12I1One2One: I12I12I1One2One;
  I12C1One2Manies: I12I12C1One2Manies;
  I12C1Many2One: I12I12C1Many2One;
  I12AllorsDateTime: I12I12AllorsDateTime;
  I2I2Many2One: I2I2I2Many2One;
  I2C1Many2One: I2I2C1Many2One;
  I2I12Many2One: I2I2I12Many2One;
  I2AllorsBoolean: I2I2AllorsBoolean;
  I2C1One2Manies: I2I2C1One2Manies;
  I2C1One2One: I2I2C1One2One;
  I2AllorsDecimal: I2I2AllorsDecimal;
  I2I2Many2Manies: I2I2I2Many2Manies;
  I2AllorsBinary: I2I2AllorsBinary;
  I2AllorsUnique: I2I2AllorsUnique;
  I2I1Many2One: I2I2I1Many2One;
  I2AllorsDateTime: I2I2AllorsDateTime;
  I2I12One2Manies: I2I2I12One2Manies;
  I2I12One2One: I2I2I12One2One;
  I2C2Many2Manies: I2I2C2Many2Manies;
  I2I1Many2Manies: I2I2I1Many2Manies;
  I2C2Many2One: I2I2C2Many2One;
  I2AllorsString: I2I2AllorsString;
  I2C2One2Manies: I2I2C2One2Manies;
  I2I1One2One: I2I2I1One2One;
  I2I1One2Manies: I2I2I1One2Manies;
  I2I12Many2Manies: I2I2I12Many2Manies;
  I2I2One2One: I2I2I2One2One;
  I2AllorsInteger: I2I2AllorsInteger;
  I2I2One2Manies: I2I2I2One2Manies;
  I2C1Many2Manies: I2I2C1Many2Manies;
  I2C2One2One: I2I2C2One2One;
  I2AllorsDouble: I2I2AllorsDouble;

  C1sWhereC1I12Many2Many: C1sWhereC1I12Many2Many;
  C1sWhereC1I12Many2One: C1sWhereC1I12Many2One;
  C1WhereC1I12One2Many: C1WhereC1I12One2Many;
  C1WhereC1I12One2One: C1WhereC1I12One2One;
  C1sWhereC1I2Many2Many: C1sWhereC1I2Many2Many;
  C1sWhereC1I2Many2One: C1sWhereC1I2Many2One;
  C1WhereC1I2One2Many: C1WhereC1I2One2Many;
  C1WhereC1I2One2One: C1WhereC1I2One2One;
  C2sWhereC2I12Many2One: C2sWhereC2I12Many2One;
  C2WhereC2I12One2One: C2WhereC2I12One2One;
  C2WhereC2I2One2One: C2WhereC2I2One2One;
  C2sWhereC2I2Many2Many: C2sWhereC2I2Many2Many;
  C2sWhereC2I12Many2Many: C2sWhereC2I12Many2Many;
  C2WhereC2I12One2Many: C2WhereC2I12One2Many;
  C2WhereC2I2One2Many: C2WhereC2I2One2Many;
  C2sWhereC2I2Many2One: C2sWhereC2I2Many2One;
  I1sWhereI1I12Many2Many: I1sWhereI1I12Many2Many;
  I1sWhereI1I2Many2Many: I1sWhereI1I2Many2Many;
  I1sWhereI1I2Many2One: I1sWhereI1I2Many2One;
  I1sWhereI1I12Many2One: I1sWhereI1I12Many2One;
  I1WhereI1I2One2Many: I1WhereI1I2One2Many;
  I1WhereI1I12One2One: I1WhereI1I12One2One;
  I1WhereI1I2One2One: I1WhereI1I2One2One;
  I1WhereI1I12One2Many: I1WhereI1I12One2Many;
  I12sWhereI12I12Many2Many: I12sWhereI12I12Many2Many;
  I12sWhereI12I2Many2Many: I12sWhereI12I2Many2Many;
  I12WhereI12I12One2Many: I12WhereI12I12One2Many;
  I12sWhereI12I2Many2One: I12sWhereI12I2Many2One;
  I12WhereI12I12One2One: I12WhereI12I12One2One;
  I12WhereI12I2One2One: I12WhereI12I2One2One;
  I12sWhereDependency: I12sWhereDependency;
  I12WhereI12I2One2Many: I12WhereI12I2One2Many;
  I12sWhereI12I12Many2One: I12sWhereI12I12Many2One;
  I2sWhereI2I2Many2One: I2sWhereI2I2Many2One;
  I2sWhereI2I12Many2One: I2sWhereI2I12Many2One;
  I2sWhereI2I2Many2Many: I2sWhereI2I2Many2Many;
  I2WhereI2I12One2Many: I2WhereI2I12One2Many;
  I2WhereI2I12One2One: I2WhereI2I12One2One;
  I2sWhereI2I12Many2Many: I2sWhereI2I12Many2Many;
  I2WhereI2I2One2One: I2WhereI2I2One2One;
  I2WhereI2I2One2Many: I2WhereI2I2One2Many;
}

export interface S1 extends Interface {
  AsC1: C1;
  AsI1: I1;

  C2WhereS1One2One: C2WhereS1One2One;
}

export interface S12 extends Interface {}

export interface AccessInterface extends Interface {}

export interface AccessControl extends Class {}

export interface Login extends Class {}

export interface CreatePermission extends Class {}

export interface ExecutePermission extends Class {}

export interface ReadPermission extends Class {}

export interface WritePermission extends Class {}

export interface Role extends Class {}

export interface SecurityToken extends Class {}

export interface UserGroup extends Class {}

export interface Build extends Class {}

export interface C1 extends Class {
  ClassMethod: MethodType;

  C1AllorsBinary: C1C1AllorsBinary;
  C1AllorsBoolean: C1C1AllorsBoolean;
  C1AllorsDateTime: C1C1AllorsDateTime;
  C1DateTimeLessThan: C1C1DateTimeLessThan;
  C1DateTimeGreaterThan: C1C1DateTimeGreaterThan;
  C1DateTimeBetweenA: C1C1DateTimeBetweenA;
  C1DateTimeBetweenB: C1C1DateTimeBetweenB;
  C1AllorsDecimal: C1C1AllorsDecimal;
  C1DecimalLessThan: C1C1DecimalLessThan;
  C1DecimalGreaterThan: C1C1DecimalGreaterThan;
  C1DecimalBetweenA: C1C1DecimalBetweenA;
  C1DecimalBetweenB: C1C1DecimalBetweenB;
  C1AllorsDouble: C1C1AllorsDouble;
  C1DoubleLessThan: C1C1DoubleLessThan;
  C1DoubleGreaterThan: C1C1DoubleGreaterThan;
  C1DoubleBetweenA: C1C1DoubleBetweenA;
  C1DoubleBetweenB: C1C1DoubleBetweenB;
  C1AllorsInteger: C1C1AllorsInteger;
  C1IntegerLessThan: C1C1IntegerLessThan;
  C1IntegerGreaterThan: C1C1IntegerGreaterThan;
  C1IntegerBetweenA: C1C1IntegerBetweenA;
  C1IntegerBetweenB: C1C1IntegerBetweenB;
  C1AllorsString: C1C1AllorsString;
  C1AllorsStringEquals: C1C1AllorsStringEquals;
  AllorsStringMax: C1AllorsStringMax;
  C1AllorsUnique: C1C1AllorsUnique;
  C1C1Many2Manies: C1C1C1Many2Manies;
  C1C1Many2One: C1C1C1Many2One;
  C1C1One2Manies: C1C1C1One2Manies;
  C1C1One2One: C1C1C1One2One;
  C1C2Many2Manies: C1C1C2Many2Manies;
  C1C2Many2One: C1C1C2Many2One;
  C1C2One2Manies: C1C1C2One2Manies;
  C1C2One2One: C1C1C2One2One;
  C1I12Many2Manies: C1C1I12Many2Manies;
  C1I12Many2One: C1C1I12Many2One;
  C1I12One2Manies: C1C1I12One2Manies;
  C1I12One2One: C1C1I12One2One;
  C1I1Many2Manies: C1C1I1Many2Manies;
  C1I1Many2One: C1C1I1Many2One;
  C1I1One2Manies: C1C1I1One2Manies;
  C1I1One2One: C1C1I1One2One;
  C1I2Many2Manies: C1C1I2Many2Manies;
  C1I2Many2One: C1C1I2Many2One;
  C1I2One2Manies: C1C1I2One2Manies;
  C1I2One2One: C1C1I2One2One;
  I1I1Many2One: I1I1I1Many2One;
  I1I12Many2Manies: I1I1I12Many2Manies;
  I1I2Many2Manies: I1I1I2Many2Manies;
  I1I2Many2One: I1I1I2Many2One;
  I1AllorsString: I1I1AllorsString;
  I1I12Many2One: I1I1I12Many2One;
  I1AllorsDateTime: I1I1AllorsDateTime;
  I1I2One2Manies: I1I1I2One2Manies;
  I1C2One2Manies: I1I1C2One2Manies;
  I1C1One2One: I1I1C1One2One;
  I1AllorsInteger: I1I1AllorsInteger;
  I1C2Many2Manies: I1I1C2Many2Manies;
  I1I1One2Manies: I1I1I1One2Manies;
  I1I1Many2Manies: I1I1I1Many2Manies;
  I1AllorsBoolean: I1I1AllorsBoolean;
  I1AllorsDecimal: I1I1AllorsDecimal;
  I1I12One2One: I1I1I12One2One;
  I1I2One2One: I1I1I2One2One;
  I1C2One2One: I1I1C2One2One;
  I1C1One2Manies: I1I1C1One2Manies;
  I1AllorsBinary: I1I1AllorsBinary;
  I1C1Many2Manies: I1I1C1Many2Manies;
  I1AllorsDouble: I1I1AllorsDouble;
  I1I1One2One: I1I1I1One2One;
  I1C1Many2One: I1I1C1Many2One;
  I1I12One2Manies: I1I1I12One2Manies;
  I1C2Many2One: I1I1C2Many2One;
  I1AllorsUnique: I1I1AllorsUnique;
  I12AllorsBinary: I12I12AllorsBinary;
  I12C2One2One: I12I12C2One2One;
  I12AllorsDouble: I12I12AllorsDouble;
  I12I1Many2One: I12I12I1Many2One;
  I12AllorsString: I12I12AllorsString;
  I12I12Many2Manies: I12I12I12Many2Manies;
  I12AllorsDecimal: I12I12AllorsDecimal;
  I12I2Many2Manies: I12I12I2Many2Manies;
  I12C2Many2Manies: I12I12C2Many2Manies;
  I12I1Many2Manies: I12I12I1Many2Manies;
  I12I12One2Manies: I12I12I12One2Manies;
  Name: I12Name;
  I12C1Many2Manies: I12I12C1Many2Manies;
  I12I2Many2One: I12I12I2Many2One;
  I12AllorsUnique: I12I12AllorsUnique;
  I12AllorsInteger: I12I12AllorsInteger;
  I12I1One2Manies: I12I12I1One2Manies;
  I12C1One2One: I12I12C1One2One;
  I12I12One2One: I12I12I12One2One;
  I12I2One2One: I12I12I2One2One;
  Dependencies: I12Dependencies;
  I12I2One2Manies: I12I12I2One2Manies;
  I12C2Many2One: I12I12C2Many2One;
  I12I12Many2One: I12I12I12Many2One;
  I12AllorsBoolean: I12I12AllorsBoolean;
  I12I1One2One: I12I12I1One2One;
  I12C1One2Manies: I12I12C1One2Manies;
  I12C1Many2One: I12I12C1Many2One;
  I12AllorsDateTime: I12I12AllorsDateTime;

  C1sWhereC1C1Many2Many: C1sWhereC1C1Many2Many;
  C1sWhereC1C1Many2One: C1sWhereC1C1Many2One;
  C1WhereC1C1One2Many: C1WhereC1C1One2Many;
  C1WhereC1C1One2One: C1WhereC1C1One2One;
  C1sWhereC1I12Many2Many: C1sWhereC1I12Many2Many;
  C1sWhereC1I12Many2One: C1sWhereC1I12Many2One;
  C1WhereC1I12One2Many: C1WhereC1I12One2Many;
  C1WhereC1I12One2One: C1WhereC1I12One2One;
  C1sWhereC1I1Many2Many: C1sWhereC1I1Many2Many;
  C1sWhereC1I1Many2One: C1sWhereC1I1Many2One;
  C1WhereC1I1One2Many: C1WhereC1I1One2Many;
  C1WhereC1I1One2One: C1WhereC1I1One2One;
  C2WhereC2C1One2One: C2WhereC2C1One2One;
  C2sWhereC2I12Many2One: C2sWhereC2I12Many2One;
  C2WhereC2I12One2One: C2WhereC2I12One2One;
  C2sWhereC2I1Many2Many: C2sWhereC2I1Many2Many;
  C2WhereC2I1One2Many: C2WhereC2I1One2Many;
  C2sWhereC2I12Many2Many: C2sWhereC2I12Many2Many;
  C2sWhereC2I1Many2One: C2sWhereC2I1Many2One;
  C2WhereC2I1One2One: C2WhereC2I1One2One;
  C2sWhereC2C1Many2Many: C2sWhereC2C1Many2Many;
  C2WhereC2I12One2Many: C2WhereC2I12One2Many;
  C2sWhereC2C1Many2One: C2sWhereC2C1Many2One;
  C2WhereC2C1One2Many: C2WhereC2C1One2Many;
  C2WhereS1One2One: C2WhereS1One2One;
  I1sWhereI1I1Many2One: I1sWhereI1I1Many2One;
  I1sWhereI1I12Many2Many: I1sWhereI1I12Many2Many;
  I1sWhereI1I12Many2One: I1sWhereI1I12Many2One;
  I1WhereI1C1One2One: I1WhereI1C1One2One;
  I1WhereI1I1One2Many: I1WhereI1I1One2Many;
  I1sWhereI1I1Many2Many: I1sWhereI1I1Many2Many;
  I1WhereI1I12One2One: I1WhereI1I12One2One;
  I1WhereI1C1One2Many: I1WhereI1C1One2Many;
  I1sWhereI1C1Many2Many: I1sWhereI1C1Many2Many;
  I1WhereI1I1One2One: I1WhereI1I1One2One;
  I1sWhereI1C1Many2One: I1sWhereI1C1Many2One;
  I1WhereI1I12One2Many: I1WhereI1I12One2Many;
  I12sWhereI12I1Many2One: I12sWhereI12I1Many2One;
  I12sWhereI12I12Many2Many: I12sWhereI12I12Many2Many;
  I12sWhereI12I1Many2Many: I12sWhereI12I1Many2Many;
  I12WhereI12I12One2Many: I12WhereI12I12One2Many;
  I12sWhereI12C1Many2Many: I12sWhereI12C1Many2Many;
  I12WhereI12I1One2Many: I12WhereI12I1One2Many;
  I12WhereI12C1One2One: I12WhereI12C1One2One;
  I12WhereI12I12One2One: I12WhereI12I12One2One;
  I12sWhereDependency: I12sWhereDependency;
  I12sWhereI12I12Many2One: I12sWhereI12I12Many2One;
  I12WhereI12I1One2One: I12WhereI12I1One2One;
  I12WhereI12C1One2Many: I12WhereI12C1One2Many;
  I12sWhereI12C1Many2One: I12sWhereI12C1Many2One;
  I2sWhereI2C1Many2One: I2sWhereI2C1Many2One;
  I2sWhereI2I12Many2One: I2sWhereI2I12Many2One;
  I2WhereI2C1One2Many: I2WhereI2C1One2Many;
  I2WhereI2C1One2One: I2WhereI2C1One2One;
  I2sWhereI2I1Many2One: I2sWhereI2I1Many2One;
  I2WhereI2I12One2Many: I2WhereI2I12One2Many;
  I2WhereI2I12One2One: I2WhereI2I12One2One;
  I2sWhereI2I1Many2Many: I2sWhereI2I1Many2Many;
  I2WhereI2I1One2One: I2WhereI2I1One2One;
  I2WhereI2I1One2Many: I2WhereI2I1One2Many;
  I2sWhereI2I12Many2Many: I2sWhereI2I12Many2Many;
  I2sWhereI2C1Many2Many: I2sWhereI2C1Many2Many;
}

export interface C2 extends Class {
  C2AllorsDecimal: C2C2AllorsDecimal;
  C2C1One2One: C2C2C1One2One;
  C2C2Many2One: C2C2C2Many2One;
  C2AllorsUnique: C2C2AllorsUnique;
  C2I12Many2One: C2C2I12Many2One;
  C2I12One2One: C2C2I12One2One;
  C2I1Many2Manies: C2C2I1Many2Manies;
  C2AllorsDouble: C2C2AllorsDouble;
  C2I1One2Manies: C2C2I1One2Manies;
  C2I2One2One: C2C2I2One2One;
  C2AllorsInteger: C2C2AllorsInteger;
  C2I2Many2Manies: C2C2I2Many2Manies;
  C2I12Many2Manies: C2C2I12Many2Manies;
  C2C2One2Manies: C2C2C2One2Manies;
  C2AllorsBoolean: C2C2AllorsBoolean;
  C2I1Many2One: C2C2I1Many2One;
  C2I1One2One: C2C2I1One2One;
  C2C1Many2Manies: C2C2C1Many2Manies;
  C2I12One2Manies: C2C2I12One2Manies;
  C2I2One2Manies: C2C2I2One2Manies;
  C2C2One2One: C2C2C2One2One;
  C2AllorsString: C2C2AllorsString;
  C2C1Many2One: C2C2C1Many2One;
  C2C2Many2Manies: C2C2C2Many2Manies;
  C2AllorsDateTime: C2C2AllorsDateTime;
  C2I2Many2One: C2C2I2Many2One;
  C2C1One2Manies: C2C2C1One2Manies;
  C2AllorsBinary: C2C2AllorsBinary;
  S1One2One: C2S1One2One;
  I12AllorsBinary: I12I12AllorsBinary;
  I12C2One2One: I12I12C2One2One;
  I12AllorsDouble: I12I12AllorsDouble;
  I12I1Many2One: I12I12I1Many2One;
  I12AllorsString: I12I12AllorsString;
  I12I12Many2Manies: I12I12I12Many2Manies;
  I12AllorsDecimal: I12I12AllorsDecimal;
  I12I2Many2Manies: I12I12I2Many2Manies;
  I12C2Many2Manies: I12I12C2Many2Manies;
  I12I1Many2Manies: I12I12I1Many2Manies;
  I12I12One2Manies: I12I12I12One2Manies;
  Name: I12Name;
  I12C1Many2Manies: I12I12C1Many2Manies;
  I12I2Many2One: I12I12I2Many2One;
  I12AllorsUnique: I12I12AllorsUnique;
  I12AllorsInteger: I12I12AllorsInteger;
  I12I1One2Manies: I12I12I1One2Manies;
  I12C1One2One: I12I12C1One2One;
  I12I12One2One: I12I12I12One2One;
  I12I2One2One: I12I12I2One2One;
  Dependencies: I12Dependencies;
  I12I2One2Manies: I12I12I2One2Manies;
  I12C2Many2One: I12I12C2Many2One;
  I12I12Many2One: I12I12I12Many2One;
  I12AllorsBoolean: I12I12AllorsBoolean;
  I12I1One2One: I12I12I1One2One;
  I12C1One2Manies: I12I12C1One2Manies;
  I12C1Many2One: I12I12C1Many2One;
  I12AllorsDateTime: I12I12AllorsDateTime;
  I2I2Many2One: I2I2I2Many2One;
  I2C1Many2One: I2I2C1Many2One;
  I2I12Many2One: I2I2I12Many2One;
  I2AllorsBoolean: I2I2AllorsBoolean;
  I2C1One2Manies: I2I2C1One2Manies;
  I2C1One2One: I2I2C1One2One;
  I2AllorsDecimal: I2I2AllorsDecimal;
  I2I2Many2Manies: I2I2I2Many2Manies;
  I2AllorsBinary: I2I2AllorsBinary;
  I2AllorsUnique: I2I2AllorsUnique;
  I2I1Many2One: I2I2I1Many2One;
  I2AllorsDateTime: I2I2AllorsDateTime;
  I2I12One2Manies: I2I2I12One2Manies;
  I2I12One2One: I2I2I12One2One;
  I2C2Many2Manies: I2I2C2Many2Manies;
  I2I1Many2Manies: I2I2I1Many2Manies;
  I2C2Many2One: I2I2C2Many2One;
  I2AllorsString: I2I2AllorsString;
  I2C2One2Manies: I2I2C2One2Manies;
  I2I1One2One: I2I2I1One2One;
  I2I1One2Manies: I2I2I1One2Manies;
  I2I12Many2Manies: I2I2I12Many2Manies;
  I2I2One2One: I2I2I2One2One;
  I2AllorsInteger: I2I2AllorsInteger;
  I2I2One2Manies: I2I2I2One2Manies;
  I2C1Many2Manies: I2I2C1Many2Manies;
  I2C2One2One: I2I2C2One2One;
  I2AllorsDouble: I2I2AllorsDouble;

  C1sWhereC1C2Many2Many: C1sWhereC1C2Many2Many;
  C1sWhereC1C2Many2One: C1sWhereC1C2Many2One;
  C1WhereC1C2One2Many: C1WhereC1C2One2Many;
  C1WhereC1C2One2One: C1WhereC1C2One2One;
  C1sWhereC1I12Many2Many: C1sWhereC1I12Many2Many;
  C1sWhereC1I12Many2One: C1sWhereC1I12Many2One;
  C1WhereC1I12One2Many: C1WhereC1I12One2Many;
  C1WhereC1I12One2One: C1WhereC1I12One2One;
  C1sWhereC1I2Many2Many: C1sWhereC1I2Many2Many;
  C1sWhereC1I2Many2One: C1sWhereC1I2Many2One;
  C1WhereC1I2One2Many: C1WhereC1I2One2Many;
  C1WhereC1I2One2One: C1WhereC1I2One2One;
  C2sWhereC2C2Many2One: C2sWhereC2C2Many2One;
  C2sWhereC2I12Many2One: C2sWhereC2I12Many2One;
  C2WhereC2I12One2One: C2WhereC2I12One2One;
  C2WhereC2I2One2One: C2WhereC2I2One2One;
  C2sWhereC2I2Many2Many: C2sWhereC2I2Many2Many;
  C2sWhereC2I12Many2Many: C2sWhereC2I12Many2Many;
  C2WhereC2C2One2Many: C2WhereC2C2One2Many;
  C2WhereC2I12One2Many: C2WhereC2I12One2Many;
  C2WhereC2I2One2Many: C2WhereC2I2One2Many;
  C2WhereC2C2One2One: C2WhereC2C2One2One;
  C2sWhereC2C2Many2Many: C2sWhereC2C2Many2Many;
  C2sWhereC2I2Many2One: C2sWhereC2I2Many2One;
  I1sWhereI1I12Many2Many: I1sWhereI1I12Many2Many;
  I1sWhereI1I2Many2Many: I1sWhereI1I2Many2Many;
  I1sWhereI1I2Many2One: I1sWhereI1I2Many2One;
  I1sWhereI1I12Many2One: I1sWhereI1I12Many2One;
  I1WhereI1I2One2Many: I1WhereI1I2One2Many;
  I1WhereI1C2One2Many: I1WhereI1C2One2Many;
  I1sWhereI1C2Many2Many: I1sWhereI1C2Many2Many;
  I1WhereI1I12One2One: I1WhereI1I12One2One;
  I1WhereI1I2One2One: I1WhereI1I2One2One;
  I1WhereI1C2One2One: I1WhereI1C2One2One;
  I1WhereI1I12One2Many: I1WhereI1I12One2Many;
  I1sWhereI1C2Many2One: I1sWhereI1C2Many2One;
  I12WhereI12C2One2One: I12WhereI12C2One2One;
  I12sWhereI12I12Many2Many: I12sWhereI12I12Many2Many;
  I12sWhereI12I2Many2Many: I12sWhereI12I2Many2Many;
  I12sWhereI12C2Many2Many: I12sWhereI12C2Many2Many;
  I12WhereI12I12One2Many: I12WhereI12I12One2Many;
  I12sWhereI12I2Many2One: I12sWhereI12I2Many2One;
  I12WhereI12I12One2One: I12WhereI12I12One2One;
  I12WhereI12I2One2One: I12WhereI12I2One2One;
  I12sWhereDependency: I12sWhereDependency;
  I12WhereI12I2One2Many: I12WhereI12I2One2Many;
  I12sWhereI12C2Many2One: I12sWhereI12C2Many2One;
  I12sWhereI12I12Many2One: I12sWhereI12I12Many2One;
  I2sWhereI2I2Many2One: I2sWhereI2I2Many2One;
  I2sWhereI2I12Many2One: I2sWhereI2I12Many2One;
  I2sWhereI2I2Many2Many: I2sWhereI2I2Many2Many;
  I2WhereI2I12One2Many: I2WhereI2I12One2Many;
  I2WhereI2I12One2One: I2WhereI2I12One2One;
  I2sWhereI2C2Many2Many: I2sWhereI2C2Many2Many;
  I2sWhereI2C2Many2One: I2sWhereI2C2Many2One;
  I2WhereI2C2One2Many: I2WhereI2C2One2Many;
  I2sWhereI2I12Many2Many: I2sWhereI2I12Many2Many;
  I2WhereI2I2One2One: I2WhereI2I2One2One;
  I2WhereI2I2One2Many: I2WhereI2I2One2Many;
  I2WhereI2C2One2One: I2WhereI2C2One2One;
}

export interface ClassWithoutRoles extends Class {}

export interface Data extends Class {
  AutocompleteFilter: DataAutocompleteFilter;
  AutocompleteOptions: DataAutocompleteOptions;
  Checkbox: DataCheckbox;
  Chips: DataChips;
  String: DataString;
  Decimal: DataDecimal;
  Date: DataDate;
  DateTime: DataDateTime;
  DateTime2: DataDateTime2;
  RadioGroup: DataRadioGroup;
  Slider: DataSlider;
  SlideToggle: DataSlideToggle;
  PlainText: DataPlainText;
  Markdown: DataMarkdown;
  Html: DataHtml;

  OrganisationsWhereOneData: OrganisationsWhereOneData;
  OrganisationsWhereManyData: OrganisationsWhereManyData;
}

export interface AA extends Class {}

export interface BB extends Class {}

export interface CC extends Class {}

export interface ValidationC1 extends Class {}

export interface ValidationC2 extends Class {}

export interface MediaTyped extends Class {}

export interface Organisation extends Class {
  JustDoIt: MethodType;
  ToggleCanWrite: MethodType;
  Delete: MethodType;

  Employees: OrganisationEmployees;
  Manager: OrganisationManager;
  Owner: OrganisationOwner;
  Shareholders: OrganisationShareholders;
  Name: OrganisationName;
  CycleOne: OrganisationCycleOne;
  CycleMany: OrganisationCycleMany;
  OneData: OrganisationOneData;
  ManyDatas: OrganisationManyDatas;
  JustDidIt: OrganisationJustDidIt;
  JustDidItDerived: OrganisationJustDidItDerived;
  UniqueId: UniquelyIdentifiableUniqueId;

  PeopleWhereCycleOne: PeopleWhereCycleOne;
  PeopleWhereCycleMany: PeopleWhereCycleMany;
}

export interface Person extends Class {
  Delete: MethodType;

  FirstName: PersonFirstName;
  MiddleName: PersonMiddleName;
  LastName: PersonLastName;
  BirthDate: PersonBirthDate;
  WorkspaceFullName: PersonWorkspaceFullName;
  SessionFullName: PersonSessionFullName;
  DomainFullName: PersonDomainFullName;
  DomainGreeting: PersonDomainGreeting;
  IsStudent: PersonIsStudent;
  Weight: PersonWeight;
  CycleOne: PersonCycleOne;
  CycleMany: PersonCycleMany;
  UniqueId: UniquelyIdentifiableUniqueId;
  UserName: UserUserName;
  InUserPassword: UserInUserPassword;
  UserEmail: UserUserEmail;

  DatasWhereAutocompleteFilter: DatasWhereAutocompleteFilter;
  DatasWhereAutocompleteOptions: DatasWhereAutocompleteOptions;
  DataWhereChip: DataWhereChip;
  OrganisationWhereEmployee: OrganisationWhereEmployee;
  OrganisationWhereManager: OrganisationWhereManager;
  OrganisationsWhereOwner: OrganisationsWhereOwner;
  OrganisationsWhereShareholder: OrganisationsWhereShareholder;
  OrganisationsWhereCycleOne: OrganisationsWhereCycleOne;
  OrganisationsWhereCycleMany: OrganisationsWhereCycleMany;
  SessionOrganisationWhereSessionDatabaseEmployee: SessionOrganisationWhereSessionDatabaseEmployee;
  SessionOrganisationWhereSessionDatabaseManager: SessionOrganisationWhereSessionDatabaseManager;
  SessionOrganisationsWhereSessionDatabaseOwner: SessionOrganisationsWhereSessionDatabaseOwner;
  SessionOrganisationsWhereSessionDatabaseShareholder: SessionOrganisationsWhereSessionDatabaseShareholder;
  WorkspaceOrganisationWhereWorkspaceDatabaseEmployee: WorkspaceOrganisationWhereWorkspaceDatabaseEmployee;
  WorkspaceOrganisationWhereWorkspaceDatabaseManager: WorkspaceOrganisationWhereWorkspaceDatabaseManager;
  WorkspaceOrganisationsWhereWorkspaceDatabaseOwner: WorkspaceOrganisationsWhereWorkspaceDatabaseOwner;
  WorkspaceOrganisationsWhereWorkspaceDatabaseShareholder: WorkspaceOrganisationsWhereWorkspaceDatabaseShareholder;
}

export interface AccessClass extends Class {}

export interface UnitSample extends Class {
  AllorsBinary: UnitSampleAllorsBinary;
  AllorsDateTime: UnitSampleAllorsDateTime;
  AllorsBoolean: UnitSampleAllorsBoolean;
  AllorsDouble: UnitSampleAllorsDouble;
  AllorsInteger: UnitSampleAllorsInteger;
  AllorsString: UnitSampleAllorsString;
  AllorsUnique: UnitSampleAllorsUnique;
  AllorsDecimal: UnitSampleAllorsDecimal;
}

export interface WorkspaceNoneObject1 extends Class {}

export interface WorkspaceNonObject2 extends Class {}

export interface WorkspaceXObject1 extends Class {}

export interface WorkspaceXObject2 extends Class {}

export interface WorkspaceXYObject1 extends Class {}

export interface WorkspaceXYObject2 extends Class {}

export interface WorkspaceYObject1 extends Class {}

export interface WorkspaceYObject2 extends Class {}

export interface SessionOrganisation extends Class {
  SessionDatabaseEmployees: SessionOrganisationSessionDatabaseEmployees;
  SessionDatabaseManager: SessionOrganisationSessionDatabaseManager;
  SessionDatabaseOwner: SessionOrganisationSessionDatabaseOwner;
  SessionDatabaseShareholders: SessionOrganisationSessionDatabaseShareholders;
  SessionWorkspaceEmployees: SessionOrganisationSessionWorkspaceEmployees;
  SessionWorkspaceManager: SessionOrganisationSessionWorkspaceManager;
  SessionWorkspaceOwner: SessionOrganisationSessionWorkspaceOwner;
  SessionWorkspaceShareholders: SessionOrganisationSessionWorkspaceShareholders;
  SessionSessionEmployees: SessionOrganisationSessionSessionEmployees;
  SessionSessionManager: SessionOrganisationSessionSessionManager;
  SessionSessionOwner: SessionOrganisationSessionSessionOwner;
  SessionSessionShareholders: SessionOrganisationSessionSessionShareholders;
}

export interface SessionPerson extends Class {
  FirstName: SessionPersonFirstName;
  LastName: SessionPersonLastName;
  FullName: SessionPersonFullName;

  SessionOrganisationWhereSessionSessionEmployee: SessionOrganisationWhereSessionSessionEmployee;
  SessionOrganisationWhereSessionSessionManager: SessionOrganisationWhereSessionSessionManager;
  SessionOrganisationsWhereSessionSessionOwner: SessionOrganisationsWhereSessionSessionOwner;
  SessionOrganisationsWhereSessionSessionShareholder: SessionOrganisationsWhereSessionSessionShareholder;
}

export interface WorkspaceOrganisation extends Class {
  WorkspaceDatabaseEmployees: WorkspaceOrganisationWorkspaceDatabaseEmployees;
  WorkspaceDatabaseManager: WorkspaceOrganisationWorkspaceDatabaseManager;
  WorkspaceDatabaseOwner: WorkspaceOrganisationWorkspaceDatabaseOwner;
  WorkspaceDatabaseShareholders: WorkspaceOrganisationWorkspaceDatabaseShareholders;
  WorkspaceWorkspaceEmployees: WorkspaceOrganisationWorkspaceWorkspaceEmployees;
  WorkspaceWorkspaceManager: WorkspaceOrganisationWorkspaceWorkspaceManager;
  WorkspaceWorkspaceOwner: WorkspaceOrganisationWorkspaceWorkspaceOwner;
  WorkspaceWorkspaceShareholders: WorkspaceOrganisationWorkspaceWorkspaceShareholders;
}

export interface WorkspacePerson extends Class {
  FirstName: WorkspacePersonFirstName;
  LastName: WorkspacePersonLastName;
  FullName: WorkspacePersonFullName;

  SessionOrganisationWhereSessionWorkspaceEmployee: SessionOrganisationWhereSessionWorkspaceEmployee;
  SessionOrganisationWhereSessionWorkspaceManager: SessionOrganisationWhereSessionWorkspaceManager;
  SessionOrganisationsWhereSessionWorkspaceOwner: SessionOrganisationsWhereSessionWorkspaceOwner;
  SessionOrganisationsWhereSessionWorkspaceShareholder: SessionOrganisationsWhereSessionWorkspaceShareholder;
  WorkspaceOrganisationWhereWorkspaceWorkspaceEmployee: WorkspaceOrganisationWhereWorkspaceWorkspaceEmployee;
  WorkspaceOrganisationWhereWorkspaceWorkspaceManager: WorkspaceOrganisationWhereWorkspaceWorkspaceManager;
  WorkspaceOrganisationsWhereWorkspaceWorkspaceOwner: WorkspaceOrganisationsWhereWorkspaceWorkspaceOwner;
  WorkspaceOrganisationsWhereWorkspaceWorkspaceShareholder: WorkspaceOrganisationsWhereWorkspaceWorkspaceShareholder;
}

export interface UniquelyIdentifiableWhereUniqueId extends AssociationType {
  ObjectType: UniquelyIdentifiable;
}

export interface UserWhereUserName extends AssociationType {
  ObjectType: User;
}

export interface UserWhereInUserPassword extends AssociationType {
  ObjectType: User;
}

export interface UserWhereUserEmail extends AssociationType {
  ObjectType: User;
}

export interface I1sWhereI1I1Many2One extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1I12Many2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1I2Many2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1I2Many2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsString extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1I12Many2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsDateTime extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I2One2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1C2One2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1C1One2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsInteger extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1C2Many2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I1One2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1I1Many2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsBoolean extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsDecimal extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I12One2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I2One2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1C2One2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1C1One2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsBinary extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1C1Many2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsDouble extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I1One2One extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1C1Many2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1I12One2Many extends AssociationType {
  ObjectType: I1;
}

export interface I1sWhereI1C2Many2One extends AssociationType {
  ObjectType: I1;
}

export interface I1WhereI1AllorsUnique extends AssociationType {
  ObjectType: I1;
}

export interface I12WhereI12AllorsBinary extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12C2One2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsDouble extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I1Many2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsString extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I12Many2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsDecimal extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I2Many2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12C2Many2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I1Many2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I12One2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereName extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12C1Many2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I2Many2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsUnique extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsInteger extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I1One2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12C1One2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I12One2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I2One2One extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereDependency extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I2One2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12C2Many2One extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12I12Many2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsBoolean extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12I1One2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12C1One2Many extends AssociationType {
  ObjectType: I12;
}

export interface I12sWhereI12C1Many2One extends AssociationType {
  ObjectType: I12;
}

export interface I12WhereI12AllorsDateTime extends AssociationType {
  ObjectType: I12;
}

export interface I2sWhereI2I2Many2One extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2C1Many2One extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2I12Many2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsBoolean extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2C1One2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2C1One2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsDecimal extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2I2Many2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsBinary extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsUnique extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2I1Many2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsDateTime extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I12One2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I12One2One extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2C2Many2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2I1Many2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2C2Many2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsString extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2C2One2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I1One2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I1One2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2I12Many2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I2One2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsInteger extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2I2One2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2sWhereI2C1Many2Many extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2C2One2One extends AssociationType {
  ObjectType: I2;
}

export interface I2WhereI2AllorsDouble extends AssociationType {
  ObjectType: I2;
}

export interface C1WhereC1AllorsBinary extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsBoolean extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsDateTime extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DateTimeLessThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DateTimeGreaterThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DateTimeBetweenA extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DateTimeBetweenB extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsDecimal extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DecimalLessThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DecimalGreaterThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DecimalBetweenA extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DecimalBetweenB extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsDouble extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DoubleLessThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DoubleGreaterThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DoubleBetweenA extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1DoubleBetweenB extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsInteger extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1IntegerLessThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1IntegerGreaterThan extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1IntegerBetweenA extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1IntegerBetweenB extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsString extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsStringEquals extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereAllorsStringMax extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1AllorsUnique extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1C1Many2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1C1Many2One extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1C1One2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1C1One2One extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1C2Many2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1C2Many2One extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1C2One2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1C2One2One extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I12Many2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I12Many2One extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I12One2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I12One2One extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I1Many2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I1Many2One extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I1One2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I1One2One extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I2Many2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1sWhereC1I2Many2One extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I2One2Many extends AssociationType {
  ObjectType: C1;
}

export interface C1WhereC1I2One2One extends AssociationType {
  ObjectType: C1;
}

export interface C2WhereC2AllorsDecimal extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2C1One2One extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2C2Many2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsUnique extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I12Many2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I12One2One extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I1Many2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsDouble extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I1One2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I2One2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsInteger extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I2Many2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I12Many2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2C2One2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsBoolean extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I1Many2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I1One2One extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2C1Many2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I12One2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2I2One2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2C2One2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsString extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2C1Many2One extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2C2Many2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsDateTime extends AssociationType {
  ObjectType: C2;
}

export interface C2sWhereC2I2Many2One extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2C1One2Many extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereC2AllorsBinary extends AssociationType {
  ObjectType: C2;
}

export interface C2WhereS1One2One extends AssociationType {
  ObjectType: C2;
}

export interface DatasWhereAutocompleteFilter extends AssociationType {
  ObjectType: Data;
}

export interface DatasWhereAutocompleteOptions extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereCheckbox extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereChip extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereString extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereDecimal extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereDate extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereDateTime extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereDateTime2 extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereRadioGroup extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereSlider extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereSlideToggle extends AssociationType {
  ObjectType: Data;
}

export interface DataWherePlainText extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereMarkdown extends AssociationType {
  ObjectType: Data;
}

export interface DataWhereHtml extends AssociationType {
  ObjectType: Data;
}

export interface OrganisationWhereEmployee extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationWhereManager extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereOwner extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereShareholder extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationWhereName extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereCycleOne extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereCycleMany extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereOneData extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationsWhereManyData extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationWhereJustDidIt extends AssociationType {
  ObjectType: Organisation;
}

export interface OrganisationWhereJustDidItDerived extends AssociationType {
  ObjectType: Organisation;
}

export interface PersonWhereFirstName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereMiddleName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereLastName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereBirthDate extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereWorkspaceFullName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereSessionFullName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereDomainFullName extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereDomainGreeting extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereIsStudent extends AssociationType {
  ObjectType: Person;
}

export interface PersonWhereWeight extends AssociationType {
  ObjectType: Person;
}

export interface PeopleWhereCycleOne extends AssociationType {
  ObjectType: Person;
}

export interface PeopleWhereCycleMany extends AssociationType {
  ObjectType: Person;
}

export interface UnitSampleWhereAllorsBinary extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsDateTime extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsBoolean extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsDouble extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsInteger extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsString extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsUnique extends AssociationType {
  ObjectType: UnitSample;
}

export interface UnitSampleWhereAllorsDecimal extends AssociationType {
  ObjectType: UnitSample;
}

export interface SessionOrganisationWhereSessionDatabaseEmployee extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationWhereSessionDatabaseManager extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionDatabaseOwner extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionDatabaseShareholder extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationWhereSessionWorkspaceEmployee extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationWhereSessionWorkspaceManager extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionWorkspaceOwner extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionWorkspaceShareholder extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationWhereSessionSessionEmployee extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationWhereSessionSessionManager extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionSessionOwner extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionOrganisationsWhereSessionSessionShareholder extends AssociationType {
  ObjectType: SessionOrganisation;
}

export interface SessionPersonWhereFirstName extends AssociationType {
  ObjectType: SessionPerson;
}

export interface SessionPersonWhereLastName extends AssociationType {
  ObjectType: SessionPerson;
}

export interface SessionPersonWhereFullName extends AssociationType {
  ObjectType: SessionPerson;
}

export interface WorkspaceOrganisationWhereWorkspaceDatabaseEmployee extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationWhereWorkspaceDatabaseManager extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationsWhereWorkspaceDatabaseOwner extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationsWhereWorkspaceDatabaseShareholder extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationWhereWorkspaceWorkspaceEmployee extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationWhereWorkspaceWorkspaceManager extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationsWhereWorkspaceWorkspaceOwner extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspaceOrganisationsWhereWorkspaceWorkspaceShareholder extends AssociationType {
  ObjectType: WorkspaceOrganisation;
}

export interface WorkspacePersonWhereFirstName extends AssociationType {
  ObjectType: WorkspacePerson;
}

export interface WorkspacePersonWhereLastName extends AssociationType {
  ObjectType: WorkspacePerson;
}

export interface WorkspacePersonWhereFullName extends AssociationType {
  ObjectType: WorkspacePerson;
}

export type UniquelyIdentifiableUniqueId = RoleType;

export type UserUserName = RoleType;

export type UserInUserPassword = RoleType;

export type UserUserEmail = RoleType;

export interface I1I1I1Many2One extends RoleType {
  ObjectType: I1;
}

export interface I1I1I12Many2Manies extends RoleType {
  ObjectType: I12;
}

export interface I1I1I2Many2Manies extends RoleType {
  ObjectType: I2;
}

export interface I1I1I2Many2One extends RoleType {
  ObjectType: I2;
}

export type I1I1AllorsString = RoleType;

export interface I1I1I12Many2One extends RoleType {
  ObjectType: I12;
}

export type I1I1AllorsDateTime = RoleType;

export interface I1I1I2One2Manies extends RoleType {
  ObjectType: I2;
}

export interface I1I1C2One2Manies extends RoleType {
  ObjectType: C2;
}

export interface I1I1C1One2One extends RoleType {
  ObjectType: C1;
}

export type I1I1AllorsInteger = RoleType;

export interface I1I1C2Many2Manies extends RoleType {
  ObjectType: C2;
}

export interface I1I1I1One2Manies extends RoleType {
  ObjectType: I1;
}

export interface I1I1I1Many2Manies extends RoleType {
  ObjectType: I1;
}

export type I1I1AllorsBoolean = RoleType;

export type I1I1AllorsDecimal = RoleType;

export interface I1I1I12One2One extends RoleType {
  ObjectType: I12;
}

export interface I1I1I2One2One extends RoleType {
  ObjectType: I2;
}

export interface I1I1C2One2One extends RoleType {
  ObjectType: C2;
}

export interface I1I1C1One2Manies extends RoleType {
  ObjectType: C1;
}

export type I1I1AllorsBinary = RoleType;

export interface I1I1C1Many2Manies extends RoleType {
  ObjectType: C1;
}

export type I1I1AllorsDouble = RoleType;

export interface I1I1I1One2One extends RoleType {
  ObjectType: I1;
}

export interface I1I1C1Many2One extends RoleType {
  ObjectType: C1;
}

export interface I1I1I12One2Manies extends RoleType {
  ObjectType: I12;
}

export interface I1I1C2Many2One extends RoleType {
  ObjectType: C2;
}

export type I1I1AllorsUnique = RoleType;

export type I12I12AllorsBinary = RoleType;

export interface I12I12C2One2One extends RoleType {
  ObjectType: C2;
}

export type I12I12AllorsDouble = RoleType;

export interface I12I12I1Many2One extends RoleType {
  ObjectType: I1;
}

export type I12I12AllorsString = RoleType;

export interface I12I12I12Many2Manies extends RoleType {
  ObjectType: I12;
}

export type I12I12AllorsDecimal = RoleType;

export interface I12I12I2Many2Manies extends RoleType {
  ObjectType: I2;
}

export interface I12I12C2Many2Manies extends RoleType {
  ObjectType: C2;
}

export interface I12I12I1Many2Manies extends RoleType {
  ObjectType: I1;
}

export interface I12I12I12One2Manies extends RoleType {
  ObjectType: I12;
}

export type I12Name = RoleType;

export interface I12I12C1Many2Manies extends RoleType {
  ObjectType: C1;
}

export interface I12I12I2Many2One extends RoleType {
  ObjectType: I2;
}

export type I12I12AllorsUnique = RoleType;

export type I12I12AllorsInteger = RoleType;

export interface I12I12I1One2Manies extends RoleType {
  ObjectType: I1;
}

export interface I12I12C1One2One extends RoleType {
  ObjectType: C1;
}

export interface I12I12I12One2One extends RoleType {
  ObjectType: I12;
}

export interface I12I12I2One2One extends RoleType {
  ObjectType: I2;
}

export interface I12Dependencies extends RoleType {
  ObjectType: I12;
}

export interface I12I12I2One2Manies extends RoleType {
  ObjectType: I2;
}

export interface I12I12C2Many2One extends RoleType {
  ObjectType: C2;
}

export interface I12I12I12Many2One extends RoleType {
  ObjectType: I12;
}

export type I12I12AllorsBoolean = RoleType;

export interface I12I12I1One2One extends RoleType {
  ObjectType: I1;
}

export interface I12I12C1One2Manies extends RoleType {
  ObjectType: C1;
}

export interface I12I12C1Many2One extends RoleType {
  ObjectType: C1;
}

export type I12I12AllorsDateTime = RoleType;

export interface I2I2I2Many2One extends RoleType {
  ObjectType: I2;
}

export interface I2I2C1Many2One extends RoleType {
  ObjectType: C1;
}

export interface I2I2I12Many2One extends RoleType {
  ObjectType: I12;
}

export type I2I2AllorsBoolean = RoleType;

export interface I2I2C1One2Manies extends RoleType {
  ObjectType: C1;
}

export interface I2I2C1One2One extends RoleType {
  ObjectType: C1;
}

export type I2I2AllorsDecimal = RoleType;

export interface I2I2I2Many2Manies extends RoleType {
  ObjectType: I2;
}

export type I2I2AllorsBinary = RoleType;

export type I2I2AllorsUnique = RoleType;

export interface I2I2I1Many2One extends RoleType {
  ObjectType: I1;
}

export type I2I2AllorsDateTime = RoleType;

export interface I2I2I12One2Manies extends RoleType {
  ObjectType: I12;
}

export interface I2I2I12One2One extends RoleType {
  ObjectType: I12;
}

export interface I2I2C2Many2Manies extends RoleType {
  ObjectType: C2;
}

export interface I2I2I1Many2Manies extends RoleType {
  ObjectType: I1;
}

export interface I2I2C2Many2One extends RoleType {
  ObjectType: C2;
}

export type I2I2AllorsString = RoleType;

export interface I2I2C2One2Manies extends RoleType {
  ObjectType: C2;
}

export interface I2I2I1One2One extends RoleType {
  ObjectType: I1;
}

export interface I2I2I1One2Manies extends RoleType {
  ObjectType: I1;
}

export interface I2I2I12Many2Manies extends RoleType {
  ObjectType: I12;
}

export interface I2I2I2One2One extends RoleType {
  ObjectType: I2;
}

export type I2I2AllorsInteger = RoleType;

export interface I2I2I2One2Manies extends RoleType {
  ObjectType: I2;
}

export interface I2I2C1Many2Manies extends RoleType {
  ObjectType: C1;
}

export interface I2I2C2One2One extends RoleType {
  ObjectType: C2;
}

export type I2I2AllorsDouble = RoleType;

export type C1C1AllorsBinary = RoleType;

export type C1C1AllorsBoolean = RoleType;

export type C1C1AllorsDateTime = RoleType;

export type C1C1DateTimeLessThan = RoleType;

export type C1C1DateTimeGreaterThan = RoleType;

export type C1C1DateTimeBetweenA = RoleType;

export type C1C1DateTimeBetweenB = RoleType;

export type C1C1AllorsDecimal = RoleType;

export type C1C1DecimalLessThan = RoleType;

export type C1C1DecimalGreaterThan = RoleType;

export type C1C1DecimalBetweenA = RoleType;

export type C1C1DecimalBetweenB = RoleType;

export type C1C1AllorsDouble = RoleType;

export type C1C1DoubleLessThan = RoleType;

export type C1C1DoubleGreaterThan = RoleType;

export type C1C1DoubleBetweenA = RoleType;

export type C1C1DoubleBetweenB = RoleType;

export type C1C1AllorsInteger = RoleType;

export type C1C1IntegerLessThan = RoleType;

export type C1C1IntegerGreaterThan = RoleType;

export type C1C1IntegerBetweenA = RoleType;

export type C1C1IntegerBetweenB = RoleType;

export type C1C1AllorsString = RoleType;

export type C1C1AllorsStringEquals = RoleType;

export type C1AllorsStringMax = RoleType;

export type C1C1AllorsUnique = RoleType;

export interface C1C1C1Many2Manies extends RoleType {
  ObjectType: C1;
}

export interface C1C1C1Many2One extends RoleType {
  ObjectType: C1;
}

export interface C1C1C1One2Manies extends RoleType {
  ObjectType: C1;
}

export interface C1C1C1One2One extends RoleType {
  ObjectType: C1;
}

export interface C1C1C2Many2Manies extends RoleType {
  ObjectType: C2;
}

export interface C1C1C2Many2One extends RoleType {
  ObjectType: C2;
}

export interface C1C1C2One2Manies extends RoleType {
  ObjectType: C2;
}

export interface C1C1C2One2One extends RoleType {
  ObjectType: C2;
}

export interface C1C1I12Many2Manies extends RoleType {
  ObjectType: I12;
}

export interface C1C1I12Many2One extends RoleType {
  ObjectType: I12;
}

export interface C1C1I12One2Manies extends RoleType {
  ObjectType: I12;
}

export interface C1C1I12One2One extends RoleType {
  ObjectType: I12;
}

export interface C1C1I1Many2Manies extends RoleType {
  ObjectType: I1;
}

export interface C1C1I1Many2One extends RoleType {
  ObjectType: I1;
}

export interface C1C1I1One2Manies extends RoleType {
  ObjectType: I1;
}

export interface C1C1I1One2One extends RoleType {
  ObjectType: I1;
}

export interface C1C1I2Many2Manies extends RoleType {
  ObjectType: I2;
}

export interface C1C1I2Many2One extends RoleType {
  ObjectType: I2;
}

export interface C1C1I2One2Manies extends RoleType {
  ObjectType: I2;
}

export interface C1C1I2One2One extends RoleType {
  ObjectType: I2;
}

export type C2C2AllorsDecimal = RoleType;

export interface C2C2C1One2One extends RoleType {
  ObjectType: C1;
}

export interface C2C2C2Many2One extends RoleType {
  ObjectType: C2;
}

export type C2C2AllorsUnique = RoleType;

export interface C2C2I12Many2One extends RoleType {
  ObjectType: I12;
}

export interface C2C2I12One2One extends RoleType {
  ObjectType: I12;
}

export interface C2C2I1Many2Manies extends RoleType {
  ObjectType: I1;
}

export type C2C2AllorsDouble = RoleType;

export interface C2C2I1One2Manies extends RoleType {
  ObjectType: I1;
}

export interface C2C2I2One2One extends RoleType {
  ObjectType: I2;
}

export type C2C2AllorsInteger = RoleType;

export interface C2C2I2Many2Manies extends RoleType {
  ObjectType: I2;
}

export interface C2C2I12Many2Manies extends RoleType {
  ObjectType: I12;
}

export interface C2C2C2One2Manies extends RoleType {
  ObjectType: C2;
}

export type C2C2AllorsBoolean = RoleType;

export interface C2C2I1Many2One extends RoleType {
  ObjectType: I1;
}

export interface C2C2I1One2One extends RoleType {
  ObjectType: I1;
}

export interface C2C2C1Many2Manies extends RoleType {
  ObjectType: C1;
}

export interface C2C2I12One2Manies extends RoleType {
  ObjectType: I12;
}

export interface C2C2I2One2Manies extends RoleType {
  ObjectType: I2;
}

export interface C2C2C2One2One extends RoleType {
  ObjectType: C2;
}

export type C2C2AllorsString = RoleType;

export interface C2C2C1Many2One extends RoleType {
  ObjectType: C1;
}

export interface C2C2C2Many2Manies extends RoleType {
  ObjectType: C2;
}

export type C2C2AllorsDateTime = RoleType;

export interface C2C2I2Many2One extends RoleType {
  ObjectType: I2;
}

export interface C2C2C1One2Manies extends RoleType {
  ObjectType: C1;
}

export type C2C2AllorsBinary = RoleType;

export interface C2S1One2One extends RoleType {
  ObjectType: S1;
}

export interface DataAutocompleteFilter extends RoleType {
  ObjectType: Person;
}

export interface DataAutocompleteOptions extends RoleType {
  ObjectType: Person;
}

export type DataCheckbox = RoleType;

export interface DataChips extends RoleType {
  ObjectType: Person;
}

export type DataString = RoleType;

export type DataDecimal = RoleType;

export type DataDate = RoleType;

export type DataDateTime = RoleType;

export type DataDateTime2 = RoleType;

export type DataRadioGroup = RoleType;

export type DataSlider = RoleType;

export type DataSlideToggle = RoleType;

export type DataPlainText = RoleType;

export type DataMarkdown = RoleType;

export type DataHtml = RoleType;

export interface OrganisationEmployees extends RoleType {
  ObjectType: Person;
}

export interface OrganisationManager extends RoleType {
  ObjectType: Person;
}

export interface OrganisationOwner extends RoleType {
  ObjectType: Person;
}

export interface OrganisationShareholders extends RoleType {
  ObjectType: Person;
}

export type OrganisationName = RoleType;

export interface OrganisationCycleOne extends RoleType {
  ObjectType: Person;
}

export interface OrganisationCycleMany extends RoleType {
  ObjectType: Person;
}

export interface OrganisationOneData extends RoleType {
  ObjectType: Data;
}

export interface OrganisationManyDatas extends RoleType {
  ObjectType: Data;
}

export type OrganisationJustDidIt = RoleType;

export type OrganisationJustDidItDerived = RoleType;

export type PersonFirstName = RoleType;

export type PersonMiddleName = RoleType;

export type PersonLastName = RoleType;

export type PersonBirthDate = RoleType;

export type PersonWorkspaceFullName = RoleType;

export type PersonSessionFullName = RoleType;

export type PersonDomainFullName = RoleType;

export type PersonDomainGreeting = RoleType;

export type PersonIsStudent = RoleType;

export type PersonWeight = RoleType;

export interface PersonCycleOne extends RoleType {
  ObjectType: Organisation;
}

export interface PersonCycleMany extends RoleType {
  ObjectType: Organisation;
}

export type UnitSampleAllorsBinary = RoleType;

export type UnitSampleAllorsDateTime = RoleType;

export type UnitSampleAllorsBoolean = RoleType;

export type UnitSampleAllorsDouble = RoleType;

export type UnitSampleAllorsInteger = RoleType;

export type UnitSampleAllorsString = RoleType;

export type UnitSampleAllorsUnique = RoleType;

export type UnitSampleAllorsDecimal = RoleType;

export interface SessionOrganisationSessionDatabaseEmployees extends RoleType {
  ObjectType: Person;
}

export interface SessionOrganisationSessionDatabaseManager extends RoleType {
  ObjectType: Person;
}

export interface SessionOrganisationSessionDatabaseOwner extends RoleType {
  ObjectType: Person;
}

export interface SessionOrganisationSessionDatabaseShareholders extends RoleType {
  ObjectType: Person;
}

export interface SessionOrganisationSessionWorkspaceEmployees extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface SessionOrganisationSessionWorkspaceManager extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface SessionOrganisationSessionWorkspaceOwner extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface SessionOrganisationSessionWorkspaceShareholders extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface SessionOrganisationSessionSessionEmployees extends RoleType {
  ObjectType: SessionPerson;
}

export interface SessionOrganisationSessionSessionManager extends RoleType {
  ObjectType: SessionPerson;
}

export interface SessionOrganisationSessionSessionOwner extends RoleType {
  ObjectType: SessionPerson;
}

export interface SessionOrganisationSessionSessionShareholders extends RoleType {
  ObjectType: SessionPerson;
}

export type SessionPersonFirstName = RoleType;

export type SessionPersonLastName = RoleType;

export type SessionPersonFullName = RoleType;

export interface WorkspaceOrganisationWorkspaceDatabaseEmployees extends RoleType {
  ObjectType: Person;
}

export interface WorkspaceOrganisationWorkspaceDatabaseManager extends RoleType {
  ObjectType: Person;
}

export interface WorkspaceOrganisationWorkspaceDatabaseOwner extends RoleType {
  ObjectType: Person;
}

export interface WorkspaceOrganisationWorkspaceDatabaseShareholders extends RoleType {
  ObjectType: Person;
}

export interface WorkspaceOrganisationWorkspaceWorkspaceEmployees extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface WorkspaceOrganisationWorkspaceWorkspaceManager extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface WorkspaceOrganisationWorkspaceWorkspaceOwner extends RoleType {
  ObjectType: WorkspacePerson;
}

export interface WorkspaceOrganisationWorkspaceWorkspaceShareholders extends RoleType {
  ObjectType: WorkspacePerson;
}

export type WorkspacePersonFirstName = RoleType;

export type WorkspacePersonLastName = RoleType;

export type WorkspacePersonFullName = RoleType;
