import { IRange, importFrom, add } from '@allors/workspace/adapters/system';

describe('IRange', () => {
  describe('as undefined set', () => {
    const set: IRange = undefined;

    describe('adding an element', () => {
      const added = add(set, 0);

      it('should return an array', () => {
        expect(Array.isArray(added)).toBeTruthy();
      });

      it('should contain that element', () => {
        expect(added).toContain(0);
      });

      it('should contain other elements', () => {
        expect((added as number[]).length).toEqual(1);
      });
    });
  });

  describe('as single element set', () => {
    const set: IRange = importFrom([1]);

    describe('adding another smaller element', () => {
      const added = add(set, 0);

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
        expect(added.length).toEqual(2);
      });
    });
  });
});
