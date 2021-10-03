import { Injectable } from '@angular/core';
import { Params, Router, ActivatedRoute } from '@angular/router';
import { Observable, BehaviorSubject } from 'rxjs';
import { ObjectType } from '@allors/workspace/meta/system';
import { IPullResult, Pull } from '@allors/workspace/domain/system';

import { PanelService } from './panel.service';
import { Context, WorkspaceService } from '@allors/workspace/angular/core';

@Injectable()
export class PanelManagerService {
  context: Context;

  id: number;

  objectType: ObjectType;

  panels: PanelService[] = [];
  expanded: string;

  on$: Observable<Date>;
  private onSubject$: BehaviorSubject<Date>;

  get panelContainerClass() {
    return this.expanded ? 'expanded-panel-container' : 'collapsed-panel-container';
  }

  constructor(workspaceService: WorkspaceService, public router: Router, public route: ActivatedRoute) {
    this.context = workspaceService.contextBuilder();
    this.on$ = this.onSubject$ = new BehaviorSubject(new Date());
  }

  on() {
    this.onSubject$.next(new Date());
  }

  onPull(pulls: Pull[]): any {
    this.panels.forEach((v) => v.onPull && v.onPull(pulls));
  }

  onPulled(loaded: IPullResult): any {
    this.panels.forEach((v) => v.onPulled && v.onPulled(loaded));
  }

  toggle(name: string) {
    let panel;
    if (!this.expanded) {
      panel = name;
    }

    const queryParams: Params = Object.assign({}, this.route.snapshot.queryParams);
    queryParams.panel = panel;
    this.router.navigate(['.'], { relativeTo: this.route, queryParams, queryParamsHandling: 'merge' });
  }
}
