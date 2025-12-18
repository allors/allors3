import { Injectable } from '@angular/core';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { NavigationService } from '../navigation/navigation.service';

export interface NavigationInfo {
  tag: string;
  list: string;
  overview: string;
}

@Injectable()
export class NavigationInfoService {
  metaPopulation: MetaPopulation;

  constructor(
    private workspaceService: WorkspaceService,
    private navigationService: NavigationService
  ) {
    this.metaPopulation =
      this.workspaceService.workspace.configuration.metaPopulation;
  }

  write(allors: { [key: string]: unknown }) {
    allors['navigation'] = this.navigation;
  }

  private get navigation(): string {
    const meta: NavigationInfo[] = [...this.metaPopulation.composites].map(
      (v) => {
        return {
          tag: v.tag,
          list: this.navigationService.listUrl(v),
          overview: this.navigationService.overviewUrl(v),
        };
      }
    );

    return JSON.stringify(meta);
  }
}
