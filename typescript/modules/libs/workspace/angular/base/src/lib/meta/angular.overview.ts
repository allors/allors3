import { Composite } from '@allors/workspace/meta/system';

interface AngularOverviewExtension {
  overview?: string;
}

export function angularOverview(composite: Composite): string;
export function angularOverview(composite: Composite, overview?: string): void;
export function angularOverview(composite: Composite, overview?: string): string | void {
  if (composite == null) {
    return;
  }

  if (overview == null) {
    return (composite._ as AngularOverviewExtension).overview;
  }

  (composite._ as AngularOverviewExtension).overview = overview;
}
