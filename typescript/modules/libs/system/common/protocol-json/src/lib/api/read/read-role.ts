import { ReadValue } from './read-value';

export interface ReadRole {
  /** Relation type tag. */
  t: string;

  /** Sorted IDs for a to-many role. */
  c?: number[] | null;

  /** Object ID for a to-one role. */
  o?: number | null;

  /** Encoded unit value, including explicit null. */
  v?: ReadValue;
}
