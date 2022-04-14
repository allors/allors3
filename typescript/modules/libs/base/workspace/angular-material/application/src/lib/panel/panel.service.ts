import { Observable, of, tap } from 'rxjs';
import { Injectable } from '@angular/core';
import {
  EditBlocking,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import {
  Panel,
  PanelService,
} from '@allors/base/workspace/angular/application';

@Injectable()
export class AllorsMaterialPanelService extends PanelService {
  panels: Set<Panel>;

  override activePanel: Panel;

  constructor(private refreshService: RefreshService) {
    super();
    this.panels = new Set();
  }

  register(panel: Panel): void {
    this.panels.add(panel);
  }

  unregister(panel: Panel): void {
    this.panels.delete(panel);
  }

  startEdit(panelId: string): Observable<boolean> {
    if (panelId == null) {
      return this.stopEdit();
    }

    if (
      this.activePanel &&
      (this.activePanel as unknown as EditBlocking).stopEdit
    ) {
      return (this.activePanel as unknown as EditBlocking).stopEdit().pipe(
        tap((success) => {
          if (success) {
            this.enable(panelId);
          }
        })
      );
    } else {
      this.enable(panelId);
      return of(true);
    }
  }

  stopEdit(): Observable<boolean> {
    if (
      this.activePanel &&
      (this.activePanel as unknown as EditBlocking).stopEdit
    ) {
      return (this.activePanel as unknown as EditBlocking).stopEdit().pipe(
        tap((success) => {
          if (success) {
            this.disable();
          }
        })
      );
    } else {
      this.disable();
      return of(true);
    }
  }

  private enable(panelId: string) {
    window.setTimeout((v) => {
      for (const panel of this.panels) {
        if (panel.panelMode === 'View') {
          panel.panelEnabled = panel.panelKind === 'Summary' ? true : false;
        } else {
          const isActivePanel = panel.panelId === panelId;
          if (isActivePanel) {
            this.activePanel = panel;
          }

          panel.panelEnabled = isActivePanel ? true : false;
        }
      }

      this.refreshService.refresh();
    }, 1000);
  }

  private disable() {
    for (const panel of this.panels) {
      panel.panelEnabled = panel.panelMode === 'View' ? true : false;
    }

    this.activePanel = null;

    this.refreshService.refresh();
  }
}
