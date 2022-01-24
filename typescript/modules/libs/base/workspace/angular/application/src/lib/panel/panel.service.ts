import { Injectable } from '@angular/core';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { PanelManagerService } from './panel-manager.service';

@Injectable()
export class PanelService {
  name: string;
  title: string;
  icon: string;
  expandable: boolean;

  onPull: (pulls: Pull[]) => void;
  onPulled: (loaded: IPullResult) => void;

  constructor(public manager: PanelManagerService) {
    manager.panels.push(this);
  }

  get isCollapsed(): boolean {
    return !this.manager.expanded;
  }

  get isExpanded(): boolean {
    return this.manager.expanded === this.name;
  }

  toggle() {
    this.manager.toggle(this.name);
  }
}
