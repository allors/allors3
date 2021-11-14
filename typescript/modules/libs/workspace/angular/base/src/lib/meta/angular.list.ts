import { Composite } from '@allors/workspace/meta/system';

interface AngularListExtension {
  list?: string;
}

export function angularList(composite: Composite, list?: string) {
  if (list == null) {
    return (composite._ as AngularListExtension).list;
  }

  (composite._ as AngularListExtension).list = list;
}
