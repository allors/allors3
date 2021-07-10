import { MapMap, Range, fromUnsorted } from '@allors/workspace/adapters/system';
import { mm } from './mm';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, Range>();

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return undefined', () => {
        expect(value).toBeUndefined();
      });
    });
  });

  describe('with key1 present', () => {
    const mapMap = new MapMap<string, string, Range>();
    const mm = ((mapMap as unknown) as { mapMap: Map<string, Map<string, Range>> }).mapMap;
    mm.set('a', new Map());

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return undefined', () => {
        expect(value).toBeUndefined();
      });
    });
  });

  describe('with key1 and key2 present', () => {
    const mapMap = new MapMap<string, string, Range>();
    mm(mapMap).set('a', new Map());
    mm(mapMap)
      .get('a')
      ?.set('b', fromUnsorted([0]));

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return value', () => {
        expect(value).toEqual(fromUnsorted([0]));
      });
    });
  });
});
