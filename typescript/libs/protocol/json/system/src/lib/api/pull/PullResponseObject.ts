export interface PullResponseObject {
  /** Id */
  i: number;

  /** Version */
  v: number;

  /** Sorted Grants */
  g: number[];

  /** Sorted Revocations */
  r: number[];
}
