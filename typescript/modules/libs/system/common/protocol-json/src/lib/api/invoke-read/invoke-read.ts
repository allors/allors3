import { Invocation } from '../invoke/invocation';
import { Response } from '../response';
import { ReadError } from '../read/read-error';
import { ReadFailure } from '../read/read-failure';
import { ReadScope } from '../read/read-scope';
import { PushReadChanged } from '../push-read/push-read';

export interface InvokeReadRequest {
  v: 1;
  i: string;
  sc: ReadScope;
  x?: string | null;
  l: Invocation[];
  refresh: boolean;
}
export type InvokeReadOutcome = Partial<Response> & {
  k: 'committed' | 'rejected' | 'unknown';
  e?: ReadError[] | null;
};
/** A trustworthy receipt is independent of the affected read payload. */
export interface InvokeReadReceipt {
  i: string;
  sc?: ReadScope | null;
  invoke: InvokeReadOutcome;
}
export interface InvokeReadResponse extends InvokeReadReceipt {
  changed?: PushReadChanged | ReadFailure | null;
}
