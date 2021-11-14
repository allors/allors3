import { Composite, RoleType } from '@allors/workspace/meta/system';

interface AngularDisplayNameExtension {
  displayName?: string;
}

export function angularDisplayName(meta: Composite | RoleType, displayName?: string) {
  if (displayName == null) {
    return (meta._ as AngularDisplayNameExtension).displayName;
  }

  (meta._ as AngularDisplayNameExtension).displayName = displayName;
}
