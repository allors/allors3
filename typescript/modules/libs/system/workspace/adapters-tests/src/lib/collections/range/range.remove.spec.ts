import { DefaultNumberRanges, IRange } from '@allors/system/workspace/adapters';

describe('IRange', () => {
  describe('as undefined set', () => {
    const ranges = new DefaultNumberRanges();
    const set: IRange<number> = undefined;

    describe('removing a non existing element', () => {
      const removed = ranges.remove(set, 0);

      it('should return undefined', () => {
        expect(removed).toBeUndefined();
      });
    });
  });

  describe('as single element set', () => {
    const ranges = new DefaultNumberRanges();
    const set: IRange<number> = ranges.importFrom([1]);

    describe('removing the element', () => {
      const removed = ranges.remove(set, 1);

      it('should return undefined', () => {
        expect(removed).toBeUndefined();
      });
    });

    describe('removing a non existing element', () => {
      const removed = ranges.remove(set, 0);

      it('should return an array', () => {
        expect(Array.isArray(removed)).toBeTruthy();
      });

      it('should contain one element', () => {
        expect((removed as []).length).toBe(1);
      });

      it('should only contain the original element', () => {
        expect(removed).toContain(1);
      });
    });
  });
});
