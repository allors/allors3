import { unitFromJson } from '@allors/system/workspace/adapters-json';
import { UnitTags } from '@allors/system/workspace/meta';

test('Integer unit values are validated when decoding JSON', () => {
  expect(unitFromJson(UnitTags.Integer, -2147483648)).toBe(-2147483648);
  expect(unitFromJson(UnitTags.Integer, 0)).toBe(0);
  expect(unitFromJson(UnitTags.Integer, 2147483647)).toBe(2147483647);
  expect(unitFromJson(UnitTags.Integer, null)).toBeNull();
  expect(unitFromJson(UnitTags.Integer, undefined)).toBeNull();

  for (const value of [
    1.5,
    -2147483649,
    2147483648,
    '12',
    true,
    false,
    NaN,
    -Infinity,
    Infinity,
  ]) {
    let hasErrors: boolean;

    try {
      unitFromJson(UnitTags.Integer, value);
      hasErrors = false;
    } catch (e) {
      hasErrors = true;
    }

    expect(hasErrors).toBeTruthy();
  }
});
