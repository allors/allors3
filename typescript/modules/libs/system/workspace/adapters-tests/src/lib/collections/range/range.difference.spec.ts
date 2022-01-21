import { DefaultNumberRanges, IRange } from '@allors/system/workspace/adapters';

describe('IRange', () => {
  describe('as undefined set', () => {
    const ranges = new DefaultNumberRanges();
    const self: IRange<number> = undefined;

    describe('difference with self', () => {
      const diff = ranges.difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another undefined set', () => {
      const other: IRange<number> = undefined;
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another single element set', () => {
      const other = ranges.importFrom([0]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another multiple element set', () => {
      const other = ranges.importFrom([3, 1, 6, 5]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });
  });

  describe('as single element set', () => {
    const ranges = new DefaultNumberRanges();
    const self: IRange<number> = [1];

    describe('difference with self', () => {
      const diff = ranges.difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with an undefined set', () => {
      const other: IRange<number> = undefined;
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another single element set with same element', () => {
      const other = ranges.importFrom([1]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another single element set with another element', () => {
      const other = ranges.importFrom([0]);
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other = ranges.importFrom([3, 0, 6, 5]);
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other = ranges.importFrom([3, 0, 1, 6, 5]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });
  });

  describe('as multiple element set', () => {
    const ranges = new DefaultNumberRanges();
    const self = ranges.importFrom([3, 0, 6, 5]);

    describe('difference with self', () => {
      const diff = ranges.difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with an undefined set', () => {
      const other: IRange<number> = undefined;
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another single element set with same element', () => {
      const other = ranges.importFrom([3]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toEqual([0, 5, 6]);
      });
    });

    describe('difference with another single element set with another element', () => {
      const other = ranges.importFrom([1]);
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other = ranges.importFrom([1, 2, 4, 7]);
      const diff = ranges.difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all same elements', () => {
      const other = ranges.importFrom([3, 0, 6, 5]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another multiple element set with some same elements', () => {
      const other = ranges.importFrom([3, 5]);
      const diff = ranges.difference(self, other);

      it('should return undefined', () => {
        expect(diff).toEqual([0, 6]);
      });
    });
  });
});
