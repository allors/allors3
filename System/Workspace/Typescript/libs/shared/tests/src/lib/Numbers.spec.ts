import { Numbers, init, add, remove, has } from '@allors/shared/system';

describe('Numbers', () => {
  describe('initialize with undefined', () => {
    const set = init(undefined);

    it('should return undefined', () => {
      expect(set).toBeUndefined();
    });
  });
  describe('as undefined set', () => {
    const set: Numbers = undefined;

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

    describe('removing a non existing element', () => {
      const removed = remove(set, 0);

      it('should return undefined', () => {
        expect(removed).toBeUndefined();
      });
    });
  });

  describe('as single element set', () => {
    const set: Numbers = init([1]);

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
