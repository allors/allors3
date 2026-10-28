import { Unit } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';

interface M extends LazyMetaPopulation {
  Binary: Unit;

  Boolean: Unit;

  DateTime: Unit;

  Decimal: Unit;

  Float: Unit;

  Integer: Unit;

  String: Unit;

  Unique: Unit;
}

describe('Unit in MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new LazyMetaPopulation({}) as M;

    it('should have Binary unit', () => {
      expect(metaPopulation.Binary).toBeDefined();
      expect(metaPopulation.Binary.singularName).toBe('Binary');
      expect(metaPopulation.Binary.tag).toBe('1');
    });

    it('should have Boolean unit', () => {
      expect(metaPopulation.Boolean).toBeDefined();
      expect(metaPopulation.Boolean.singularName).toBe('Boolean');
      expect(metaPopulation.Boolean.tag).toBe('2');
    });

    it('should have DateTime unit', () => {
      expect(metaPopulation.DateTime).toBeDefined();
      expect(metaPopulation.DateTime.singularName).toBe('DateTime');
      expect(metaPopulation.DateTime.tag).toBe('3');
    });

    it('should have Decimal unit', () => {
      expect(metaPopulation.Decimal).toBeDefined();
      expect(metaPopulation.Decimal.singularName).toBe('Decimal');
      expect(metaPopulation.Decimal.tag).toBe('4');
    });

    it('should have Float unit', () => {
      expect(metaPopulation.Float).toBeDefined();
      expect(metaPopulation.Float.singularName).toBe('Float');
      expect(metaPopulation.Float.tag).toBe('5');
    });

    it('should have Integer unit', () => {
      expect(metaPopulation.Integer).toBeDefined();
      expect(metaPopulation.Integer.singularName).toBe('Integer');
      expect(metaPopulation.Integer.tag).toBe('6');
    });

    it('should have String unit', () => {
      expect(metaPopulation.String).toBeDefined();
      expect(metaPopulation.String.singularName).toBe('String');
      expect(metaPopulation.String.tag).toBe('7');
    });

    it('should have Unique unit', () => {
      expect(metaPopulation.Unique).toBeDefined();
      expect(metaPopulation.Unique.singularName).toBe('Unique');
      expect(metaPopulation.Unique.tag).toBe('8');
    });
  });
});
