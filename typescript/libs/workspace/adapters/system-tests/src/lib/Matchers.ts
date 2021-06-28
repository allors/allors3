import { expect } from '@jest/globals';
import { IObject } from '@allors/workspace/domain/system';

expect.extend({
  toEqualObjects(objects: IObject[], names: string[]) {
    const objectNames = objects?.map((v) => v['Name']);
    const sortedObjectNames = objectNames.sort();
    const sortedNames = names.slice().sort();

    const pass =
      sortedObjectNames.length === sortedNames.length &&
      sortedObjectNames.every(function (value, index) {
        return value === sortedNames[index];
      });

    if (pass) {
      return { pass, message: `Expected ${sortedObjectNames} not to equal ${sortedNames}` };
    }

    return { pass, message: `Expected ${sortedObjectNames} to equal ${sortedNames}` };
  },
});

declare global {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace jest {
    interface Matchers<R> {
      toEqualObjects: (argument: string[]) => R;
    }
  }
}
