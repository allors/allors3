import {
  Grant,
  MemoryCache,
  Operations,
  Permission,
  RecordChangedEvent,
  Revocation,
} from '@allors/system/workspace/connection';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import { FakeRecord } from '../fakes/fake-record';

// The memory cache on its own: what it keeps, the version guard on set, the record-changed
// event and the hooks.
describe('MemoryCache', () => {
  let m: M;

  beforeEach(() => {
    m = new LazyMetaPopulation(data) as unknown as M;
  });

  it('holds a record by id', () => {
    const cache = new MemoryCache('Default', m);
    const record = new FakeRecord(m.C1, 1, 1);

    expect(cache.setRecord(record)).toBe(true);

    expect(cache.getRecord(1)).toBe(record);
    expect(cache.getRecord(2)).toBeUndefined();
  });

  it('refuses an older version', () => {
    const cache = new MemoryCache('Default', m);
    const newer = new FakeRecord(m.C1, 1, 2);
    const older = new FakeRecord(m.C1, 1, 1);

    cache.setRecord(newer);

    expect(cache.setRecord(older)).toBe(false);
    expect(cache.getRecord(1)).toBe(newer);
  });

  it('replaces by the same or a newer version', () => {
    const cache = new MemoryCache('Default', m);
    const first = new FakeRecord(m.C1, 1, 1);
    const same = new FakeRecord(m.C1, 1, 1);
    const newer = new FakeRecord(m.C1, 1, 2);

    cache.setRecord(first);

    expect(cache.setRecord(same)).toBe(true);
    expect(cache.getRecord(1)).toBe(same);

    expect(cache.setRecord(newer)).toBe(true);
    expect(cache.getRecord(1)).toBe(newer);
  });

  it('raises recordChanged when a record is replaced', () => {
    const cache = new MemoryCache('Default', m);
    const changed: RecordChangedEvent[] = [];
    cache.recordChanged.subscribe((e) => changed.push(e));

    const first = new FakeRecord(m.C1, 1, 1);
    cache.setRecord(first);
    expect(changed).toEqual([]);

    const newer = new FakeRecord(m.C1, 1, 2);
    cache.setRecord(newer);
    expect(changed.length).toBe(1);
    expect(changed[0].id).toBe(1);
    expect(changed[0].record).toBe(newer);

    cache.setRecord(first);
    expect(changed.length).toBe(1);
  });

  it('forgets a record on removeRecord', () => {
    const cache = new MemoryCache('Default', m);
    cache.setRecord(new FakeRecord(m.C1, 1, 1));

    cache.removeRecord(1);

    expect(cache.getRecord(1)).toBeUndefined();
    cache.removeRecord(1);
  });

  it('forgets everything on clear', () => {
    const cache = new MemoryCache('Default', m);
    cache.setRecord(new FakeRecord(m.C1, 1, 1));
    cache.setGrant(new Grant(10, 1, [100]));
    cache.setRevocation(new Revocation(20, 1, [101]));
    cache.setPermission(
      new Permission(100, m.C1, m.C1.C1AllorsString, Operations.Read)
    );

    cache.clear();

    expect(cache.getRecord(1)).toBeUndefined();
    expect(cache.getGrant(10)).toBeUndefined();
    expect(cache.getRevocation(20)).toBeUndefined();
    expect(cache.hasPermission(100)).toBe(false);
    expect(
      cache.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(0);
  });

  it('keeps the newest version of a grant and a revocation', () => {
    const cache = new MemoryCache('Default', m);

    const newerGrant = new Grant(10, 2, [100]);
    cache.setGrant(newerGrant);
    cache.setGrant(new Grant(10, 1, [101]));
    expect(cache.getGrant(10)).toBe(newerGrant);

    const newerRevocation = new Revocation(20, 2, [100]);
    cache.setRevocation(newerRevocation);
    cache.setRevocation(new Revocation(20, 1, [101]));
    expect(cache.getRevocation(20)).toBe(newerRevocation);

    const newestGrant = new Grant(10, 3, [102]);
    cache.setGrant(newestGrant);
    expect(cache.getGrant(10)).toBe(newestGrant);
  });

  it('finds a permission by class, operand type and operation', () => {
    const cache = new MemoryCache('Default', m);

    cache.setPermission(
      new Permission(100, m.C1, m.C1.C1AllorsString, Operations.Read)
    );
    cache.setPermission(
      new Permission(101, m.C1, m.C1.C1AllorsString, Operations.Write)
    );
    cache.setPermission(
      new Permission(102, m.C1, m.C1.ClassMethod, Operations.Execute)
    );

    expect(cache.hasPermission(100)).toBe(true);
    expect(cache.hasPermission(103)).toBe(false);
    expect(
      cache.getPermission(m.C1, m.C1.C1AllorsString, Operations.Read)
    ).toBe(100);
    expect(
      cache.getPermission(m.C1, m.C1.C1AllorsString, Operations.Write)
    ).toBe(101);
    expect(
      cache.getPermission(m.C1, m.C1.ClassMethod, Operations.Execute)
    ).toBe(102);
    expect(
      cache.getPermission(m.C1, m.C1.C1AllorsInteger, Operations.Read)
    ).toBe(0);
    expect(
      cache.getPermission(m.C2, m.C1.C1AllorsString, Operations.Read)
    ).toBe(0);
  });

  it('knows the workspace name and the meta population it serves', () => {
    const cache = new MemoryCache('Default', m);

    expect(cache.workspaceName).toBe('Default');
    expect(cache.metaPopulation).toBe(m);
  });
});
