import { Range, importFrom,  difference } from '@allors/workspace/adapters/system';

describe('Range', () => {
  describe('as undefined set', () => {
    const self: Range = undefined;

    describe('difference with self', () => {
      const diff = difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another undefined set', () => {
      const other: Range = undefined;
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another single element set', () => {
      const other: Range = importFrom([0]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another multiple element set', () => {
      const other: Range = importFrom([3, 1, 6, 5]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });
  });

  describe('as single element set', () => {
    const self: Range = [1];

    describe('difference with self', () => {
      const diff = difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with an undefined set', () => {
      const other: Range = undefined;
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another single element set with same element', () => {
      const other: Range = importFrom([1]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another single element set with another element', () => {
      const other: Range = importFrom([0]);
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other: Range = importFrom([3, 0, 6, 5]);
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other: Range = importFrom([3, 0, 1, 6, 5]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });
  });

  describe('as multiple element set', () => {
    const self: Range = importFrom([3, 0, 6, 5]);

    describe('difference with self', () => {
      const diff = difference(self, self);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with an undefined set', () => {
      const other: Range = undefined;
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another single element set with same element', () => {
      const other: Range = importFrom([3]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toEqual([0, 5, 6]);
      });
    });

    describe('difference with another single element set with another element', () => {
      const other: Range = importFrom([1]);
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all different elements', () => {
      const other: Range = importFrom([1, 2, 4, 7]);
      const diff = difference(self, other);

      it('should return self', () => {
        expect(diff).toEqual(self);
      });
    });

    describe('difference with another multiple element set with all same elements', () => {
      const other: Range = importFrom([3, 0, 6, 5]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toBeUndefined();
      });
    });

    describe('difference with another multiple element set with some same elements', () => {
      const other: Range = importFrom([3, 5]);
      const diff = difference(self, other);

      it('should return undefined', () => {
        expect(diff).toEqual([0, 6]);
      });
    });
  });
});
