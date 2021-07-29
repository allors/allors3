export interface PullResponseObject {
  /** Id */
  i: number;

  /** Version */
  v: number;

  /** AccessControls (Range) */
  a: number[];

  /** DeniedPermissions (Range) */
  d: number[];
}
