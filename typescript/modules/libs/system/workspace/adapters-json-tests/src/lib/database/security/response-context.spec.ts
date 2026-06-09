import { ResponseContext } from '@allors/system/workspace/adapters-json';

test('checkForMissingRevocations flags ids absent from revocationById', () => {
  const database = {
    ranges: { enumerate: (value: number[]) => value },
    grantById: new Map<number, unknown>(),
    revocationById: new Map<number, unknown>([[1, {}]]), // revocation 1 is cached
    permissions: new Set<number>([2]), // decoy: id 2 is a permission, not a revocation
  } as any;

  const context = new ResponseContext(database);
  context.checkForMissingRevocations([1, 2] as any);

  // Only 2 is an uncached revocation; 1 is already in revocationById. The bug checked
  // `permissions` instead, so it would flag 1 (absent from permissions) and miss 2.
  expect([...context.missingRevocationIds]).toEqual([2]);
});
