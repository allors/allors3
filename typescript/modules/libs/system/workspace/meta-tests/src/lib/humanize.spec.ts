import { humanize } from '@allors/system/workspace/meta';

describe('humanize', function () {
  describe('camelCased', function () {
    it('returns humanized', function () {
      const camelCased = 'camelCase';
      const humanized = humanize(camelCased);

      expect(humanized).toBe('Camel Case');
    });
  });
});
