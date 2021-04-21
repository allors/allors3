import { IMetaPopulation, IUnit } from '@allors/workspace/system';
import { MetaPopulation } from '@allors/workspace/json';

interface ITypedMetaPopulation extends IMetaPopulation {
  Binary: IUnit;

  Boolean: IUnit;

  DateTime: IUnit;

  Decimal: IUnit;

  Float: IUnit;

  Integer: IUnit;

  String: IUnit;

  Unique: IUnit;
}

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new MetaPopulation(null) as ITypedMetaPopulation;

    it('should be newable', () => {
      expect(metaPopulation).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Binary).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Boolean).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.DateTime).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Decimal).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Float).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Integer).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.String).not.toBeNull();
    });

    it('should have Binary unit', () => {
      expect(metaPopulation.Unique).not.toBeNull();
    });
  });
});
