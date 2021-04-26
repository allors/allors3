import { LazyMetaPopulation } from '@allors/meta/lazy/system';
import { data, M } from '@allors/meta/generated';

describe('Meta', () => {
  const m = (new LazyMetaPopulation(data) as unknown) as M;

  // it('is defined', () => {
  //   expect(m).toBeDefined();
  // });

  // it('metaInterfaces should be defined', () => {
  //   data.interfaces.forEach((v) => {
  //     expect(m[v.name]).toBeDefined();
  //   });
  // });

  // it('metaClasses should be defined', () => {
  //   data.classes.forEach((v) => {
  //     expect(m[v.name]).toBeDefined();
  //   });
  // });

  // it('metaObject.objectType should be defined', () => {
  //   data.classes.concat(data.interfaces).forEach((v) => {
  //     const metaObjectType: ObjectType = m[v.name];
  //     expect(metaObjectType).toBeDefined();
  //   });
  // });

  // it('metaObject roleTypes should be defined', () => {
  //   data.classes.concat(data.interfaces).forEach((v) => {
  //     const metaObjectType: ObjectType & { [key: string]: any } = m[v.name];
  //     const objectType = metaObjectType;

  //     const roleTypes = Object.keys(objectType.roleTypeByName).map((w) => (objectType.roleTypeByName as { [key: string]: any })[w]);

  //     roleTypes.forEach((w) => {
  //       const metaRoleType = metaObjectType[w.name];
  //       expect(metaRoleType).toBeDefined();
  //     });
  //   });
  // });

  // it('metaObject associationTypes should be defined', () => {
  //   data.classes.concat(data.interfaces).forEach((v) => {
  //     const metaObjectType: ObjectType & { [key: string]: any } = m[v.name];
  //     const objectType = metaObjectType;

  //     const associationTypes = Object.keys(objectType.associationTypeByName).map((w) => (objectType.associationTypeByName as { [key: string]: any })[w]);

  //     associationTypes.forEach((w) => {
  //       const metaAssociationType = metaObjectType[w.name];
  //       expect(metaAssociationType).toBeDefined();
  //     });
  //   });
  // });

  // it('hierarchy should be defined for roles', () => {
  //   expect(m.C1.Name).toBeDefined();
  //   expect(m.I1.Name).toBeDefined();
  //   expect(m.I12.Name).toBeDefined();
  // });

  // it('hierarchy should be defined for associations', () => {
  //   expect(m.C1.C2WhereS1One2One).toBeDefined();
  //   expect(m.I1.C2WhereS1One2One).toBeDefined();
  //   expect(m.S1.C2WhereS1One2One).toBeDefined();
  // });
});
