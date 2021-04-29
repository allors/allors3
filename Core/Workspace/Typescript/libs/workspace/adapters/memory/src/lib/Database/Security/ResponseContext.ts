import { AccessControl } from "./AccessControl";
import { Permission } from "./Permission";

export class ResponseContext {
  constructor(private readonly accessControlById: Map<number, AccessControl>, private readonly permissionById: Map<number, Permission>) {
    this.missingAccessControlIds = new Set();
    this.missingPermissionIds = new Set();
  }

  missingAccessControlIds: Set<number>;

  missingPermissionIds: Set<number>;

  checkForMissingAccessControls(value: number[]): number[] {
    if (value == null) {
      return null;
    }

    for (const accessControlId of value.filter((v) => !this.accessControlById.has(v))) {
      _ = this.missingAccessControlIds.add(accessControlId);
    }

    return value;
  }

  checkForMissingPermissions(value: number[]): number[] {
    if (value == null) {
      return null;
    }

    for (const permissionId of value.filter((v) => !this.permissionById.has(v))) {
      _ = this.missingPermissionIds.add(permissionId);
    }

    return value;
  }
}
