import { Composite, RoleType } from '@allors/workspace/meta/system';

interface AngularDisplayNameExtension {
  displayName?: string;
}

export function angularDisplayName(meta: Composite | RoleType): string;
export function angularDisplayName(meta: Composite | RoleType, displayName: string): void;
export function angularDisplayName(meta: Composite | RoleType, displayName?: string): string | void {
  if (meta == null) {
    return;
  }

  if (displayName == null) {
    return (meta._ as AngularDisplayNameExtension).displayName;
  }

  (meta._ as AngularDisplayNameExtension).displayName = displayName;
}
