import { MapMap, Range, importFrom } from '@allors/workspace/adapters/system';
import { mm } from './mm';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, Range>();

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', importFrom([1]));
      });

      it('should have that value', () => {
        const a = mm(mapMap).get('a');
        expect(a?.get('b')).toEqual(importFrom([1]));
      });
    });

    describe('setting undefined', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', importFrom());
      });

      it('should not have the map for key1', () => {
        const a = mm(mapMap).get('a');
        expect(a).toBeUndefined();
      });
    });
  });

  describe('with key1 present', () => {
    const mapMap = new MapMap<string, string, Range>();
    mm(mapMap).set('a', new Map());

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', importFrom([1]));
      });

      it('should have that value', () => {
        expect(mm(mapMap).get('a')?.get('b')).toEqual(importFrom([1]));
      });
    });
  });

  describe('with key1 and key2 present', () => {
    const mapMap = new MapMap<string, string, Range>();
    mm(mapMap).set('a', new Map());
    mm(mapMap)
      .get('a')
      ?.set('b', importFrom([0]));

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', importFrom([1]));
      });

      it('should return value', () => {
        expect(mm(mapMap).get('a')?.get('b')).toEqual(importFrom([1]));
      });
    });
  });
});
