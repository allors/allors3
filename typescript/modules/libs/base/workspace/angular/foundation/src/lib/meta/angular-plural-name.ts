import { Composite, humanize, RoleType } from '@allors/system/workspace/meta';

interface AngularPluralNameExtension {
  pluralName?: string;
}

export function angularPluralName(meta: Composite | RoleType): string;
export function angularPluralName(
  meta: Composite | RoleType,
  pluralName: string
): void;
export function angularPluralName(
  meta: Composite | RoleType,
  pluralName?: string
): string | void {
  const extension = meta?._ as AngularPluralNameExtension;

  if (pluralName == null) {
    return extension?.pluralName ?? humanize(meta.pluralName);
  }

  extension.pluralName = pluralName;
}
