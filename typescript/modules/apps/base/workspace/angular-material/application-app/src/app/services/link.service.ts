import { Injectable } from '@angular/core';
import {
  LinkService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Node } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';

@Injectable()
export class AppLinkService implements LinkService {
  linkByObjectType: Map<Composite, Node[]>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    const { treeBuilder: t } = m;

    this.linkByObjectType = new Map<Composite, Node[]>([
      [m.Organisation, t.Organisation({ Address: {} })],
      [m.Person, t.Person({ Address: {} })],
    ]);
  }

  link(objectType: Composite): Node[] {
    return this.linkByObjectType.get(objectType);
  }
}
