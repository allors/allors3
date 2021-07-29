import { IRange, importFrom, remove } from '@allors/workspace/adapters/system';

describe('IRange', () => {
  describe('as undefined set', () => {
    const set: IRange = undefined;

    describe('removing a non existing element', () => {
      const removed = remove(set, 0);

      it('should return undefined', () => {
        expect(removed).toBeUndefined();
      });
    });
  });

  describe('as single element set', () => {
    const set: IRange = importFrom([1]);

    describe('removing the element', () => {
      const removed = remove(set, 1);

      it('should return undefined', () => {
        expect(removed).toBeUndefined();
      });
    });

    describe('removing a non existing element', () => {
      const removed = remove(set, 0);

      it('should return an array', () => {
        expect(Array.isArray(removed)).toBeTruthy();
      });

      it('should contain one element', () => {
        expect((removed as []).length).toEqual(1);
      });

      it('should only contain the original element', () => {
        expect(removed).toContain(1);
      });
    });
  });
});
