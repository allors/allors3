import { Injectable } from '@angular/core';
import { Panel, PanelMode } from './panel';

@Injectable()
export class PanelService {
  panels: Set<Panel>;
  objectType: any;
  id: number;

  register(panel: Panel): void {
    this.panels.add(panel);
  }

  unregister(panel: Panel): void {
    this.panels.delete(panel);
  }

  select(panel: Panel): void {}

  toggle(panel: Panel): void {}

  get(panelId: string, panelMode: PanelMode): Panel {
    for (const panel of this.panels) {
      if (panel.panelId === panelId && panel.panelMode === panelMode) {
        return panel;
      }
    }

    return null;
  }

  constructor() {
    this.panels = new Set();
  }
}
