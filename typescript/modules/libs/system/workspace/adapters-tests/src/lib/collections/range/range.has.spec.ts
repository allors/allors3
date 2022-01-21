import { DefaultNumberRanges, IRange } from '@allors/system/workspace/adapters';

describe('IRange', () => {
  describe('as undefined set', () => {
    const ranges = new DefaultNumberRanges();
    const set: IRange<number> = undefined;

    describe('has a number', () => {
      const hasZero = ranges.has(set, 0);

      it('should return undefined', () => {
        expect(hasZero).toBeFalsy();
      });
    });
  });

  describe('as single element set', () => {
    const ranges = new DefaultNumberRanges();
    const set = ranges.importFrom([1]);

    describe('has the element', () => {
      const hasTheElement = ranges.has(set, 1);

      it('should be true', () => {
        expect(hasTheElement).toBeTruthy();
      });
    });

    describe('has a non existing element', () => {
      const hasNonExistingElement = ranges.has(set, 0);

      it('should be false', () => {
        expect(hasNonExistingElement).toBeFalsy();
      });
    });
  });
});

describe('as multiple element set', () => {
  const ranges = new DefaultNumberRanges();
  const set = ranges.importFrom([3, 1, 6, 5]);

  describe('has the elements 1, 3, 5 and 6', () => {
    const has1 = ranges.has(set, 1);
    const has3 = ranges.has(set, 3);
    const has5 = ranges.has(set, 5);
    const has6 = ranges.has(set, 6);

    it('should be true', () => {
      expect(has1).toBeTruthy();
      expect(has3).toBeTruthy();
      expect(has5).toBeTruthy();
      expect(has6).toBeTruthy();
    });
  });

  describe('has non existing elements -1, 0, 2, 4, 7,8', () => {
    const hasMin1 = ranges.has(set, -1);
    const has0 = ranges.has(set, 0);
    const has2 = ranges.has(set, 2);
    const has4 = ranges.has(set, 4);
    const has7 = ranges.has(set, 7);
    const has8 = ranges.has(set, 8);

    it('should be false', () => {
      expect(hasMin1).toBeFalsy();
      expect(has0).toBeFalsy();
      expect(has2).toBeFalsy();
      expect(has4).toBeFalsy();
      expect(has7).toBeFalsy();
      expect(has8).toBeFalsy();
    });
  });
});
