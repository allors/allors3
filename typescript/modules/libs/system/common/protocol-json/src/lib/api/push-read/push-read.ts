import { Response } from '../response';
import { PushRequestNewObject } from '../push/push-request-new-object';
import { PushRequestObject } from '../push/push-request-object';
import { PushResponseNewObject } from '../push/push-response-new-object';
import { ReadError } from '../read/read-error';
import { ReadFailure } from '../read/read-failure';
import { ReadPull } from '../read/read-pull';
import { ReadScope } from '../read/read-scope';
import { ReadRecord } from '../read/read-record';
import { ReadSecurity } from '../read/read-security';
import { ReadPermission } from '../read/read-permission';
import { DiscoverySuccess } from '../read/discovery-success';

export interface PushReadChanges {
  n?: PushRequestNewObject[] | null;
  o?: PushRequestObject[] | null;
}

export interface PushReadRequest {
  v: 1;
  i: string;
  sc: ReadScope;
  x?: string | null;
  push: PushReadChanges;
  returnChanged?: boolean | null;
  discover?: ReadPull | null;
}

export type PushReadOutcome = Partial<Response> &
  (
    | { k: 'committed'; n: PushResponseNewObject[]; e?: ReadError[] | null }
    | { k: 'rejected' | 'unknown'; n?: null; e?: ReadError[] | null }
  );

/** Independently valid even when a requested read payload is unusable. */
export interface PushReadReceipt {
  i: string;
  sc?: ReadScope | null;
  push: PushReadOutcome;
}

export interface PushReadChanged {
  i: string;
  sc: ReadScope;
  k: 'changed';
  p: ReadRecord[];
  s: ReadSecurity[];
  pm: ReadPermission[];
  u: number[];
}

export interface PushReadResponse extends PushReadReceipt {
  changed?: PushReadChanged | ReadFailure | null;
  discovery?: DiscoverySuccess | ReadFailure | null;
}
