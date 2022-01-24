import {
  MapMap,
  IRange,
  DefaultNumberRanges,
} from '@allors/system/workspace/adapters';
import { mm } from './mm';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, IRange<number>>();

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return undefined', () => {
        expect(value).toBeUndefined();
      });
    });
  });

  describe('with key1 present', () => {
    const mapMap = new MapMap<string, string, IRange<number>>();
    const mm = (
      mapMap as unknown as { mapMap: Map<string, Map<string, IRange<number>>> }
    ).mapMap;
    mm.set('a', new Map());

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return undefined', () => {
        expect(value).toBeUndefined();
      });
    });
  });

  describe('with key1 and key2 present', () => {
    const ranges = new DefaultNumberRanges();
    const mapMap = new MapMap<string, string, IRange<number>>();
    mm(mapMap).set('a', new Map());
    mm(mapMap)
      .get('a')
      ?.set('b', ranges.importFrom([0]));

    describe('getting a value', () => {
      const value = mapMap.get('a', 'b');

      it('should return value', () => {
        expect(value).toEqual(ranges.importFrom([0]));
      });
    });
  });
});
