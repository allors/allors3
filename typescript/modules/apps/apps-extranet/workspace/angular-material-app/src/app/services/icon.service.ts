import { M } from '@allors/default/workspace/meta';
import { Composite, RelationType } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';

import { Injectable } from '@angular/core';
import { IconService } from '@allors/base/workspace/angular-material/application';

@Injectable()
export class AppIconService implements IconService {
  iconByComposite: Map<Composite | RelationType, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.iconByComposite = new Map();
    this.iconByComposite.set(m.Country, 'public');
    this.iconByComposite.set(m.Organisation, 'domain');
    this.iconByComposite.set(m.Person, 'person');
  }

  icon(meta: Composite | RelationType): string {
    return this.iconByComposite.get(meta);
  }
}
