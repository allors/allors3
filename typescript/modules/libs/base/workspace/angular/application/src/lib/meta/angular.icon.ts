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
  if (composite == null) {
    return;
  }

  if (icon == null) {
    return (composite._ as AngularIconExtension).icon;
  }

  (composite._ as AngularIconExtension).icon = icon;
}
