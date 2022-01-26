import { Composite, RoleType } from '@allors/system/workspace/meta';

interface AngularDisplayNameExtension {
  displayName?: string;
}

export function angularDisplayName(meta: Composite | RoleType): string;
export function angularDisplayName(
  meta: Composite | RoleType,
  displayName: string
): void;
export function angularDisplayName(
  meta: Composite | RoleType,
  displayName?: string
): string | void {
  const extension = meta?._ as AngularDisplayNameExtension;

  if (displayName == null) {
    return extension?.displayName;
  }

  extension.displayName = displayName;
}
