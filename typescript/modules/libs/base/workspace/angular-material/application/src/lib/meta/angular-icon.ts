import { Composite } from '@allors/system/workspace/meta';

interface AngularIconExtension {
  icon?: string;
}

export function angularIcon(composite: Composite): string;
export function angularIcon(composite: Composite, icon: string): void;
export function angularIcon(
  composite: Composite,
  icon?: string
): string | void {
  const extension = composite?._ as AngularIconExtension;

  if (icon == null) {
    return extension?.icon;
  }

  extension.icon = icon;
}
