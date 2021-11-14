import { Composite } from '@allors/workspace/meta/system';

interface AngularOverviewExtension {
  overview?: string;
}

export function angularOverview(composite: Composite, overview?: string) {
  if (overview == null) {
    return (composite._ as AngularOverviewExtension).overview;
  }

  (composite._ as AngularOverviewExtension).overview = overview;
}
