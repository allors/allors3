export enum DatabaseMode {
  NoPush,
  Push,
  PushAndPull,
  // SharedDatabase,
  // ExclusiveDatabase,
}

export const databaseModes = Object.values(DatabaseMode).filter(
  (v) => typeof v === 'number'
) as DatabaseMode[];
