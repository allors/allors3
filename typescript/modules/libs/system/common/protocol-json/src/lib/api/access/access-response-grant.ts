export interface AccessResponseGrant {
  /** Id */
  i: number;

  /** Version */
  v: number;

  /** Permission IDs; absent or null in Sync when the complete entry was advertised */
  p?: number[] | null;
}
