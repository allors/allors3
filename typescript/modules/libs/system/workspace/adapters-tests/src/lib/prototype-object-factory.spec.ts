import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';

// Regression for prototype-object-factory.ts: an object's `toJSON` must delegate to the strategy's
// (lowercase) `toJSON()`, not the non-existent PascalCase `ToJSON()`. PrototypeObjectFactory only reads
// metaPopulation.classes[].{roleTypes,associationTypes,methodTypes} and ObjectBase.init() is a no-op, so a
// minimal mock class + strategy stub suffices (no workspace/session).
describe('PrototypeObjectFactory toJSON', () => {
  it('serializes an object via the strategy toJSON()', () => {
    const cls = { roleTypes: [], associationTypes: [], methodTypes: [] } as any;
    const factory = new PrototypeObjectFactory({ classes: [cls] } as any);

    const object = factory.create({
      cls,
      id: 42,
      toJSON() {
        return { id: this.id };
      },
    } as any);

    // Before the fix this calls strategy.ToJSON() (undefined) and throws TypeError.
    // (`toJSON` is added to the prototype dynamically, so it isn't on the IObject type — cast.)
    expect((object as any).toJSON()).toEqual({ id: 42 });
    expect(JSON.parse(JSON.stringify(object))).toEqual({ id: 42 });
  });
});
