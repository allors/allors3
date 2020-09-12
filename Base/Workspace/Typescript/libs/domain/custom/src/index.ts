import { Database } from '@allors/workspace/system';

import { extend as extendBase } from "@allors/domain/base"
export function extend(workspace: Database) {
    extendBase(workspace);
}
