import { Database } from '@allors/workspace/core';

import { extend as extendBase } from "@allors/domain/base"
export function extend(database: Database) {
    extendBase(database);
}
