export interface ReadRecordHeader {
  /** Database object ID. */
  i: number;

  /** Class tag. */
  c: string;

  /** Committed data version. */
  v: number;

  /** Complete applicable grants, as indexes into the containing message's security table. */
  g: number[];

  /** Complete applicable revocations, as indexes into the containing message's security table. */
  r: number[];
}
