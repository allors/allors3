import {
  MetaService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite, pluralize } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMetaService implements MetaService {
  singularNameByComposite: Map<Composite, string>;
  pluralNameByComposite: Map<Composite, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.singularNameByComposite = new Map();
    this.pluralNameByComposite = new Map();

    this.singularNameByComposite.set(m.Organisation, 'Organizatie');
    this.singularNameByComposite.set(m.Person, 'Persoon');

    this.pluralNameByComposite.set(m.Person, 'Personen');
  }

  singularName(composite: Composite): string {
    return (
      this.singularNameByComposite.get(composite) ?? composite.singularName
    );
  }

  pluralName(composite: Composite): string {
    return (
      this.pluralNameByComposite.get(composite) ??
      pluralize(this.singularNameByComposite.get(composite)) ??
      composite.pluralName
    );
  }
}
