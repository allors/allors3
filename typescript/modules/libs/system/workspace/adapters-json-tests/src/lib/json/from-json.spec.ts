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

  expect(() => unitFromJson(UnitTags.Integer, 1.5)).toThrow(
    new Error(
      "Cannot convert number to Integer (tag '6'): 1.5 is not a whole number from -2147483648 through 2147483647."
    )
  );
  expect(() => unitFromJson(UnitTags.Integer, '12')).toThrow(
    new Error("Cannot convert string to Integer (tag '6').")
  );
});

test('DateTime unit values are validated when decoding JSON', () => {
  const date = new Date('2026-10-09T12:34:56.000Z');

  expect(unitFromJson(UnitTags.DateTime, date)).toBe(date);
  expect(unitFromJson(UnitTags.DateTime, '2026-10-09T12:34:56Z')).toEqual(date);
  expect(unitFromJson(UnitTags.DateTime, null)).toBeNull();

  expect(() => unitFromJson(UnitTags.DateTime, 'garbage')).toThrow(
    new Error(
      'Cannot convert string to DateTime (tag \'3\'): "garbage" is not a date.'
    )
  );
  expect(() => unitFromJson(UnitTags.DateTime, 0)).toThrow(
    new Error("Cannot convert number to DateTime (tag '3').")
  );
});
