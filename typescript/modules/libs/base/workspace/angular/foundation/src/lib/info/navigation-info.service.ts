import { WorkspaceService } from '@allors/workspace/angular/core';
import { Injectable } from '@angular/core';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { angularList } from '../meta/angular.list';
import { angularOverview } from '../meta/angular.overview';

export interface NavigationInfo {
  tag: string;
  list: string;
  overview: string;
}

@Injectable()
export class NavigationInfoService {
  metaPopulation: MetaPopulation;

  constructor(private workspaceService: WorkspaceService) {
    this.metaPopulation =
      this.workspaceService.workspace.configuration.metaPopulation;
  }

  write(allors: { [key: string]: unknown }) {
    allors.navigation = this.navigation;
  }

  private get navigation(): string {
    const meta: NavigationInfo[] = [...this.metaPopulation.composites].map(
      (v) => {
        return {
          tag: v.tag,
          list: angularList(v),
          overview: angularOverview(v),
        };
      }
    );

    return JSON.stringify(meta);
  }
}
