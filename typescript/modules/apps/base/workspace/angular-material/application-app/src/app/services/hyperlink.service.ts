import { Injectable } from '@angular/core';
import { Composite } from '@allors/system/workspace/meta';
import { Node, toPaths } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  HyperlinkService,
  HyperlinkType,
} from '@allors/base/workspace/angular-material/application';

function create(tree: Node[], label?: string): HyperlinkType {
  return {
    label,
    tree,
    paths: toPaths(tree),
  };
}

@Injectable()
export class AppHyperlinkService implements HyperlinkService {
  linkTypesByObjectType: Map<Composite, HyperlinkType[]>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    const { treeBuilder: t } = m;

    this.linkTypesByObjectType = new Map<Composite, HyperlinkType[]>([
      [m.Organisation, [create(t.Organisation({ Address: {} }))]],
      [m.Person, [create(t.Person({ Address: {} }))]],
    ]);
  }

  linkTypes(objectType: Composite): HyperlinkType[] {
    return this.linkTypesByObjectType.get(objectType);
  }
}
