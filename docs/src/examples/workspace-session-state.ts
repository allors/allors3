// A finite documentation model of the cases cited by the workspace session state page.
// It does not call the platform or model arbitrary workspace operations.
export interface RoleValues {
  string: string | null;
  integer: number;
  toOne: string;
  toMany: string[];
}

export type Role = keyof RoleValues;
export interface RecordSnapshot { version: number; values: RoleValues; grantVersion: number; grantAllowsRead: boolean }
export interface State {
  database: RecordSnapshot;
  connection: RecordSnapshot;
  retained: RecordSnapshot;
  latestReceived: RecordSnapshot;
  local: Partial<RoleValues>;
  pushed: boolean;
}

type Action =
  | { kind: 'start' }
  | { kind: 'edit'; values: Partial<RoleValues> }
  | { kind: 'remote-push'; values: Partial<RoleValues> }
  | { kind: 'remote-pull' | 'pull' | 'push' | 'reset' | 'permission-refresh' };

export type Outcome = 'ready' | 'edited' | 'remote-pushed' | 'refreshed' | 'accepted'
  | 'pinned' | 'version-error' | 'pushed' | 'reset' | 'permission-refreshed';

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
  refreshed: 'Connection refreshed', accepted: 'Pull accepted', pinned: 'Active snapshot pinned',
  'version-error': 'Version error', pushed: 'Push succeeded', reset: 'Reset complete',
  'permission-refreshed': 'Shared security refreshed',
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
  explanation: 'The remote session pulls the changed object, refreshing the shared connection. The editing object has not pulled: its active and latest received snapshots, local edits and diff originals remain unchanged.',
};
const reset: Step = {
  id: 'reset', title: 'Reset the editing object', action: { kind: 'reset' },
  explanation: 'Reset clears local edits and adopts the latest complete data and security snapshot already received by this session. It makes no network request. Another session’s Pull cannot change this Reset target.',
};
const recoveryPull: Step = {
  id: 'recovery-pull', title: 'Pull after Reset', action: { kind: 'pull' },
  explanation: 'This editing session has not yet received the remote snapshot. Its Pull now accepts the refreshed record and its security. The session now reads the remote values.',
};
const pushedPull: Step = {
  id: 'recovery-pull', title: 'Pull after Push', action: { kind: 'pull' },
  explanation: 'The editing Pull refreshes the connection, clears pushed local changes and accepts the new snapshot. The pushed flag is cleared and no diffs remain.',
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
      { id: 'editing-pull', title: 'Receive the newer snapshot', action: { kind: 'pull' },
        explanation: 'Pull records the latest complete snapshot for this session. The object has edits, so its active data and security remain pinned. The integer stays 1, and all three local edits and diff originals remain. Pull does not attempt a merge.' },
      reset],
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
    id: 'disjoint', title: 'Disjoint edits stay pinned',
    summary: 'Even a change to an untouched role leaves the edited object pinned. Reset adopts the latest received snapshot; the application can then reapply selected edits.',
    steps: [start, edit,
      remotePush({ integer: 2 }, 'The remote session changes only the integer to 2 and pushes. The remote values of the three locally edited roles remain unchanged.'),
      remotePull,
      { id: 'editing-pull', title: 'Receive the disjoint change', action: { kind: 'pull' },
        explanation: 'The latest received snapshot has integer 2. Because the object has local edits, its active snapshot stays at integer 1 with its original version and security. Pull succeeds without automatically merging.' },
      reset,
      { id: 'reapply', title: 'Reapply the chosen edits', action: { kind: 'edit', values: local },
        explanation: 'The application saved the chosen edits before Reset. It reapplies them through the normal permission checks against the newly adopted snapshot. The remote integer remains 2.' },
      { id: 'push', title: 'Push using the accepted version', action: { kind: 'push' },
        explanation: 'Push succeeds using the newly accepted version. The database keeps the remote integer and receives the local string and relation changes. The editing object is marked pushed, but its baseline and local edits stay in place until Pull.' },
      pushedPull],
  },
  conflict('string-conflict', 'Conflicting string', { string: 'remote' },
    'The remote session changes the string to "remote" while the editing session has "local".'),
  conflict('to-one-conflict', 'Conflicting to-one role', { toOne: 'c1D' },
    'The remote session selects c1D for the to-one role while the editing session selects c1C.'),
  conflict('to-many-conflict', 'Conflicting to-many role', { toMany: ['c1B', 'c1D'] },
    'The remote session adds c1D while the editing session adds c1C. Pull keeps the entire active snapshot pinned; Reset adopts the remote membership without combining additions.'),
  {
    id: 'push-reset', title: 'Reset after successful Push', initial: { string: null },
    summary: 'A successful Push does not accept a new baseline. Reset before Pull exposes the last pulled string, even though the database contains the pushed value.',
    steps: [
      { ...start, explanation: 'The editing session has pulled c1A with a null string. The other displayed roles use the same illustrative starting values as the editing examples.' },
      { id: 'local-edit', title: 'Change the string to "X"', action: { kind: 'edit', values: { string: 'X' } },
        explanation: 'The string reads "X" from the local edit; its diff original is null.' },
      { id: 'push', title: 'Push the string successfully', action: { kind: 'push' },
        explanation: 'The database receives "X" and the object is marked pushed. Its retained string is still null; the local value still reads "X". Push does not fetch a replacement record. CanWrite is false while the object is marked pushed.' },
      { ...reset, explanation: 'Reset clears the local string edit, exposing null from the retained record. It does not undo the database Push, fetch a record or clear the pushed flag.' },
      { ...pushedPull, explanation: 'Pull refreshes the connection and accepts the pushed string "X". The object was still marked pushed, so Pull clears the committed edits and that flag. No diffs remain.' },
    ],
  },
  {
    id: 'permissions', title: 'Data and security stay together',
    summary: 'A grant changes while the object’s data version stays the same. The editing object keeps its old security until Reset adopts the snapshot received by its own Pull.',
    steps: [start, edit,
      { id: 'permission-refresh', title: 'Another session receives changed security', action: { kind: 'permission-refresh' },
        explanation: 'The server removes read permission for the string from grant G. Another session loads this change into the shared cache. G has a new version; the object’s data version is unchanged. The editing session keeps its active snapshot and its own Reset target.' },
      { id: 'editing-pull', title: 'Receive the new security snapshot', action: { kind: 'pull' },
        explanation: 'The editing session now receives the new grant version with the corresponding filtered data. Local edits pin its active data and security, so reading the string remains allowed on that active snapshot. The latest received snapshot denies the read and contains no string value.' },
      reset,
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

function apply(state: State, action: Action): Outcome {
  switch (action.kind) {
    case 'start': return 'ready';
    case 'edit': state.local = structuredClone(action.values); return 'edited';
    case 'remote-push':
      state.database = { ...state.database, version: state.database.version + 1, values: structuredClone({ ...state.database.values, ...action.values }) };
      return 'remote-pushed';
    case 'remote-pull': state.connection = structuredClone(state.database); return 'refreshed';
    case 'pull':
      state.connection = structuredClone(state.database);
      if (!state.connection.grantAllowsRead) state.connection.values.string = null;
      state.latestReceived = structuredClone(state.connection);
      if (!state.pushed && Object.keys(state.local).length > 0) return 'pinned';
      if (state.pushed) state.local = {};
      state.pushed = false;
      state.retained = structuredClone(state.connection);
      return 'accepted';
    case 'push':
      if (state.retained.version !== state.database.version) return 'version-error';
      state.database = { ...state.database, version: state.database.version + 1, values: visibleValues(state) };
      state.pushed = true;
      return 'pushed';
    case 'reset':
      state.local = {};
      state.retained = structuredClone(state.latestReceived);
      return 'reset';
    case 'permission-refresh':
      state.database.grantVersion++;
      state.database.grantAllowsRead = false;
      state.connection = structuredClone(state.database);
      state.connection.values.string = null;
      return 'permission-refreshed';
  }
}

export function getFrames(scenario: Scenario): Frame[] {
  const record = { version: 1, values: { ...baseline, ...scenario.initial }, grantVersion: 1, grantAllowsRead: true };
  const state: State = {
    database: structuredClone(record), connection: structuredClone(record), retained: structuredClone(record),
    latestReceived: structuredClone(record), local: {}, pushed: false,
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
