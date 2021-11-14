import { Composite } from '@allors/workspace/meta/system';

interface AngularIconExtension {
  icon?: string;
}

export function angularIcon(composite: Composite, icon?: string) {
  if (icon == null) {
    return (composite._ as AngularIconExtension).icon;
  }

  (composite._ as AngularIconExtension).icon = icon;
}
