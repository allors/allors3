// A finite documentation model of the cases cited by the workspace session state page.
// It does not call the platform or model arbitrary workspace operations.
export interface RoleValues {
  string: string | null;
  integer: number;
  toOne: string;
  toMany: string[];
}

export type Role = keyof RoleValues;
export interface RecordSnapshot { version: number; values: RoleValues }
export interface State {
  database: RecordSnapshot;
  connection: RecordSnapshot;
  retained: RecordSnapshot;
  local: Partial<RoleValues>;
  pushed: boolean;
  grantAllowsRead: boolean;
}

type Action =
  | { kind: 'start' }
  | { kind: 'edit'; values: Partial<RoleValues> }
  | { kind: 'remote-push'; values: Partial<RoleValues> }
  | { kind: 'remote-pull' | 'pull' | 'push' | 'reset' | 'permission-refresh' };

export type Outcome = 'ready' | 'edited' | 'remote-pushed' | 'refreshed' | 'accepted'
  | 'merge-error' | 'version-error' | 'pushed' | 'reset' | 'permission-refreshed';

interface Step { id: string; title: string; explanation: string; action: Action }
export interface Scenario {
  id: string;
  title: string;
  summary: string;
  initial?: Partial<RoleValues>;
  steps: Step[];
}
export interface Frame extends Omit<Step, 'action'> { state: State; outcome: Outcome }

export const roles: { key: Role; label: string }[] = [
  { key: 'string', label: 'String' }, { key: 'integer', label: 'Integer' },
  { key: 'toOne', label: 'To-one' }, { key: 'toMany', label: 'To-many' },
];

export const outcomeLabels: Record<Outcome, string> = {
  ready: 'Ready', edited: 'Local edits', 'remote-pushed': 'Remote Push succeeded',
  refreshed: 'Connection refreshed', accepted: 'Pull accepted', 'merge-error': 'Merge error',
  'version-error': 'Version error', pushed: 'Push succeeded', reset: 'Reset complete',
  'permission-refreshed': 'Permission map refreshed',
};

const baseline: RoleValues = { string: 'baseline', integer: 1, toOne: 'c1B', toMany: ['c1B'] };
const local: Partial<RoleValues> = { string: 'local', toOne: 'c1C', toMany: ['c1B', 'c1C'] };

const start: Step = {
  id: 'start', title: 'Start from an accepted record', action: { kind: 'start' },
  explanation: 'Both sessions have pulled c1A and all relation targets. The editing object has accepted the connection record. No local edits exist.',
};
const edit: Step = {
  id: 'local-edit', title: 'Edit three roles locally', action: { kind: 'edit', values: local },
  explanation: 'The editing session changes the string to "local", selects c1C for the to-one role and adds c1C to the to-many role. Reads prefer these local values. Diff originals still come from the retained record.',
};
const remotePull: Step = {
  id: 'remote-pull', title: 'Pull in the remote session', action: { kind: 'remote-pull' },
  explanation: 'The remote session pulls the changed object, refreshing the shared connection. The editing object has not pulled: its retained version, integer, local edits and diff originals remain unchanged.',
};
const reset: Step = {
  id: 'reset', title: 'Reset the editing object', action: { kind: 'reset' },
  explanation: 'Reset clears local edits and their diffs. Reads expose the retained record again. Reset does not fetch data or accept the connection record.',
};
const recoveryPull: Step = {
  id: 'recovery-pull', title: 'Pull after Reset', action: { kind: 'pull' },
  explanation: 'With no local edits left to conflict, the editing Pull accepts the refreshed record and its version. The session now reads the remote values.',
};
const pushedPull: Step = {
  id: 'recovery-pull', title: 'Pull after Push', action: { kind: 'pull' },
  explanation: 'The editing Pull refreshes the connection, clears pushed local changes and accepts the new record without the ordinary merge check. The pushed flag is cleared and no diffs remain.',
};

function remotePush(values: Partial<RoleValues>, explanation: string): Step {
  return { id: 'remote-push', title: 'Push in the remote session', action: { kind: 'remote-push', values }, explanation };
}

function conflict(id: string, title: string, change: Partial<RoleValues>, reason: string): Scenario {
  return {
    id, title, summary: reason,
    steps: [start, edit,
      remotePush({ integer: 2, ...change }, `${reason} The remote session also changes the integer to 2 and pushes successfully. The connection and editing records still hold their earlier values until Pull.`),
      remotePull,
      { id: 'editing-pull', title: 'Pull encounters a conflict', action: { kind: 'pull' },
        explanation: 'Pull returns one merge error for c1A in both runtimes. One locally edited role has a different remote baseline, so the entire replacement record is rejected for this object. Even the untouched integer stays 1. All three local edits and diff originals remain.' },
      reset, recoveryPull],
  };
}

export const scenarios: Scenario[] = [
  {
    id: 'remote-refresh', title: 'Another session refreshes',
    summary: 'A newer connection record does not replace the editing object’s accepted baseline. A stale Push fails; Reset and Pull are separate steps.',
    steps: [start, edit,
      remotePush({ string: 'remote', integer: 2, toOne: 'c1D', toMany: ['c1B', 'c1D'] },
        'The remote session changes all four roles and pushes. The database now has the newer values. Push alone does not refresh either the connection record or the editing record.'),
      remotePull,
      { id: 'push', title: 'Push with the old version', action: { kind: 'push' },
        explanation: 'The editing Push uses the retained version and reports a version error for c1A. .NET returns an error result; TypeScript throws ResultError containing that result. The failed Push preserves the local edits and old baseline.' },
      reset, recoveryPull],
  },
  {
    id: 'disjoint', title: 'Disjoint edits merge',
    summary: 'A remote integer change can be accepted while local string and relation edits remain. The next Push preserves both sides’ changes.',
    steps: [start, edit,
      remotePush({ integer: 2 }, 'The remote session changes only the integer to 2 and pushes. The remote values of the three locally edited roles remain unchanged.'),
      remotePull,
      { id: 'editing-pull', title: 'Pull accepts the disjoint change', action: { kind: 'pull' },
        explanation: 'For every locally edited role, the refreshed value still matches the retained baseline, so Pull accepts the whole new record. Its integer is 2 and its version is newer. The local edits and all three diff originals are preserved.' },
      { id: 'push', title: 'Push using the accepted version', action: { kind: 'push' },
        explanation: 'Push succeeds using the newly accepted version. The database keeps the remote integer and receives the local string and relation changes. The editing object is marked pushed, but its baseline and local edits stay in place until Pull.' },
      pushedPull],
  },
  conflict('string-conflict', 'Conflicting string', { string: 'remote' },
    'The remote session changes the string to "remote" while the editing session has "local".'),
  conflict('to-one-conflict', 'Conflicting to-one role', { toOne: 'c1D' },
    'The remote session selects c1D for the to-one role while the editing session selects c1C.'),
  conflict('to-many-conflict', 'Conflicting to-many role', { toMany: ['c1B', 'c1D'] },
    'The remote session adds c1D while the editing session adds c1C. Merge compares the whole role; it does not combine these different membership additions.'),
  {
    id: 'push-reset', title: 'Reset after successful Push', initial: { string: null },
    summary: 'A successful Push does not accept a new baseline. Reset before Pull exposes the last pulled string, even though the database contains the pushed value.',
    steps: [
      { ...start, explanation: 'The editing session has pulled c1A with a null string. The other displayed roles use the same illustrative starting values as the merge examples.' },
      { id: 'local-edit', title: 'Change the string to "X"', action: { kind: 'edit', values: { string: 'X' } },
        explanation: 'The string reads "X" from the local edit; its diff original is null.' },
      { id: 'push', title: 'Push the string successfully', action: { kind: 'push' },
        explanation: 'The database receives "X" and the object is marked pushed. Its retained string is still null; the local value still reads "X". Push does not fetch a replacement record. CanWrite is false while the object is marked pushed.' },
      { ...reset, explanation: 'Reset clears the local string edit, exposing null from the retained record. It does not undo the database Push, fetch a record or clear the pushed flag.' },
      { ...pushedPull, explanation: 'Pull refreshes the connection and accepts the pushed string "X". The object was still marked pushed, so Pull bypasses the ordinary merge check and clears that flag. No diffs remain.' },
    ],
  },
  {
    id: 'permissions', title: 'A retained record is not an authorization snapshot',
    summary: 'Implementation illustration: the retained record refers to a known grant G. With that ID unchanged and no revocation, read permission depends on G in the live connection map. This is not a general permission-freshness guarantee.',
    steps: [start, edit,
      remotePush({ integer: 2 }, 'The remote session changes the integer to 2 and pushes. The retained record’s grant ID G is unchanged in this illustration.'),
      remotePull,
      { id: 'permission-refresh', title: 'Refresh the known grant map', action: { kind: 'permission-refresh' },
        explanation: 'A subsequent Sync/Access refresh through the connection updates known grant G so it no longer allows reading the string. A permission check through the editing object’s retained grant ID now denies that read, although its data baseline and local values are unchanged. The displayed values describe held data, not authorization to expose it. The security tests cover synchronized-object grant and revocation refresh; retaining an older editing record here illustrates the implementation’s live-map lookup. Pull without Sync and changed grant/revocation IDs have no general freshness guarantee.' },
    ],
  },
];

export function visibleValues(state: State): RoleValues {
  return structuredClone({ ...state.retained.values, ...state.local });
}

export function diff(state: State) {
  return roles.filter(({ key }) => Object.hasOwn(state.local, key)).map(({ key }) => ({
    role: key, original: structuredClone(state.retained.values[key]), changed: structuredClone(state.local[key]!),
  }));
}

function sameValue(left: RoleValues[Role], right: RoleValues[Role]): boolean {
  if (Array.isArray(left) && Array.isArray(right)) {
    const sortedRight = [...right].sort();
    return left.length === right.length && [...left].sort().every((value, i) => value === sortedRight[i]);
  }
  return left === right;
}

function apply(state: State, action: Action): Outcome {
  switch (action.kind) {
    case 'start': return 'ready';
    case 'edit': state.local = structuredClone(action.values); return 'edited';
    case 'remote-push':
      state.database = { version: state.database.version + 1, values: structuredClone({ ...state.database.values, ...action.values }) };
      return 'remote-pushed';
    case 'remote-pull': state.connection = structuredClone(state.database); return 'refreshed';
    case 'pull':
      state.connection = structuredClone(state.database);
      if (!state.pushed && roles.some(({ key }) => Object.hasOwn(state.local, key) &&
          !sameValue(state.retained.values[key], state.connection.values[key]))) return 'merge-error';
      if (state.pushed) state.local = {};
      state.pushed = false;
      state.retained = structuredClone(state.connection);
      return 'accepted';
    case 'push':
      if (state.retained.version !== state.database.version) return 'version-error';
      state.database = { version: state.database.version + 1, values: visibleValues(state) };
      state.pushed = true;
      return 'pushed';
    case 'reset': state.local = {}; return 'reset';
    case 'permission-refresh': state.grantAllowsRead = false; return 'permission-refreshed';
  }
}

export function getFrames(scenario: Scenario): Frame[] {
  const record = { version: 1, values: { ...baseline, ...scenario.initial } };
  const state: State = {
    database: structuredClone(record), connection: structuredClone(record), retained: structuredClone(record),
    local: {}, pushed: false, grantAllowsRead: true,
  };
  return scenario.steps.map(({ action, ...step }) => {
    const outcome = apply(state, action);
    return { ...step, outcome, state: structuredClone(state) };
  });
}

export function formatValue(role: Role, value: RoleValues[Role] | undefined): string {
  if (value === undefined) return '—';
  if (value === null) return 'null';
  if (Array.isArray(value)) return `{${value.join(', ')}}`;
  return role === 'string' ? JSON.stringify(value) : String(value);
}
