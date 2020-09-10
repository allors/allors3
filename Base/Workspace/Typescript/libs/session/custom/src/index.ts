import { Database } from '@allors/domain/system';

import { extend as extendBase } from "@allors/session/base"
export function extend(workspace: Database) {
    extendBase(workspace);
}
