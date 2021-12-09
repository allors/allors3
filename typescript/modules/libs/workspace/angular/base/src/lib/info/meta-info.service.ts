import { WorkspaceService } from '@allors/workspace/angular/core';
import { Injectable } from '@angular/core';
import { MetaPopulation } from '@allors/workspace/meta/system';
import { angularList } from '../meta/angular.list';
import { angularOverview } from '../meta/angular.overview';

export interface MetaInfo {
  tag: string;
  list: string;
  overview: string;
}

@Injectable()
export class MetaInfoService {
  metaPopulation: MetaPopulation;

  constructor(private workspaceService: WorkspaceService) {
    this.metaPopulation = this.workspaceService.workspace.configuration.metaPopulation;
  }

  write(allors: { [key: string]: unknown }) {
    allors.meta = this.meta;
  }

  private get meta(): string {
    const meta: MetaInfo[] = [...this.metaPopulation.composites].map((v) => {
      return {
        tag: v.tag,
        list: angularList(v),
        overview: angularOverview(v),
      };
    });

    return JSON.stringify(meta);
  }
}
