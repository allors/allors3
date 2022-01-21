import {
  MapMap,
  IRange,
  DefaultNumberRanges,
} from '@allors/system/workspace/adapters';
import { mm } from './mm';

describe('MapMap', () => {
  describe('after construction', () => {
    const ranges = new DefaultNumberRanges();
    const mapMap = new MapMap<string, string, IRange<number>>();

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', ranges.importFrom([1]));
      });

      it('should have that value', () => {
        const a = mm(mapMap).get('a');
        expect(a?.get('b')).toEqual(ranges.importFrom([1]));
      });
    });

    describe('setting undefined', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', ranges.importFrom());
      });

      it('should not have the map for key1', () => {
        const a = mm(mapMap).get('a');
        expect(a).toBeUndefined();
      });
    });
  });

  describe('with key1 present', () => {
    const ranges = new DefaultNumberRanges();
    const mapMap = new MapMap<string, string, IRange<number>>();
    mm(mapMap).set('a', new Map());

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', ranges.importFrom([1]));
      });

      it('should have that value', () => {
        expect(mm(mapMap).get('a')?.get('b')).toEqual(ranges.importFrom([1]));
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

    describe('setting a value', () => {
      beforeEach(() => {
        mapMap.set('a', 'b', ranges.importFrom([1]));
      });

      it('should return value', () => {
        expect(mm(mapMap).get('a')?.get('b')).toEqual(ranges.importFrom([1]));
      });
    });
  });
});
