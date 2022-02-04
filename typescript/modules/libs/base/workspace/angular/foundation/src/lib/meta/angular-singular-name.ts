import { Composite, humanize, RoleType } from '@allors/system/workspace/meta';

interface AngularSingularNameExtension {
  singularName?: string;
}

export function angularSingularName(meta: Composite | RoleType): string;
export function angularSingularName(
  meta: Composite | RoleType,
  singularName: string
): void;
export function angularSingularName(
  meta: Composite | RoleType,
  singularName?: string
): string | void {
  const extension = meta?._ as AngularSingularNameExtension;

  if (singularName == null) {
    return extension?.singularName ?? humanize(meta.singularName);
  }

  extension.singularName = singularName;
}
