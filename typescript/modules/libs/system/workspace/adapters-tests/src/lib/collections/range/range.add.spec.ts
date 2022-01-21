import { IRange, DefaultNumberRanges } from '@allors/system/workspace/adapters';

describe('IRange', () => {
  describe('as undefined set', () => {
    const ranges = new DefaultNumberRanges();
    const set: IRange<number> = undefined;

    describe('adding an element', () => {
      const added = ranges.add(set, 0);

      it('should return an array', () => {
        expect(Array.isArray(added)).toBeTruthy();
      });

      it('should contain that element', () => {
        expect(added).toContain(0);
      });

      it('should contain other elements', () => {
        expect((added as number[]).length).toBe(1);
      });
    });
  });

  describe('as single element set', () => {
    const ranges = new DefaultNumberRanges();
    const set = ranges.importFrom([1]);

    describe('adding another smaller element', () => {
      const added = ranges.add(set, 0);

      it('should return an array', () => {
        expect(Array.isArray(added)).toBeTruthy();
      });

      it('should contain that smaller element', () => {
        expect(added).toContain(0);
      });

      it('should contain that smaller element at the first index', () => {
        expect(added[0]).toBe(0);
      });

      it('should contain two elements', () => {
        expect(added.length).toBe(2);
      });
    });
  });
});
