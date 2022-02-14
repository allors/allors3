import { Injectable } from '@angular/core';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  LinkService,
  LinkType,
} from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { Node, toPaths } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';

function create(tree: Node[], label?: string): LinkType {
  return {
    label,
    tree,
    paths: toPaths(tree),
  };
}

@Injectable()
export class AppLinkService implements LinkService {
  linkTypesByObjectType: Map<Composite, LinkType[]>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    const { treeBuilder: t } = m;

    this.linkTypesByObjectType = new Map<Composite, LinkType[]>([
      [m.Organisation, [create(t.Organisation({ CurrentContacts: {} }))]],
      [m.Person, [create(t.Person({ BankAccounts: {} }))]],
    ]);
  }

  linkTypes(objectType: Composite): LinkType[] {
    return this.linkTypesByObjectType.get(objectType);
  }
}
