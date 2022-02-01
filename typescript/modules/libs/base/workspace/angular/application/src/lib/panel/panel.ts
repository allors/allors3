import { Observable } from 'rxjs';

export type PanelMode = 'Edit' | 'View';

export type PanelKind = 'Summary' | 'Detail' | 'Relationship';

export interface Panel {
  panelMode: PanelMode;

  panelKind: PanelKind;

  panelId: string;

  panelEnabled: boolean;
}

export interface EditPanel {
  panelMode: 'Edit';

  panelStopEdit(): Observable<boolean>;
}

export interface ViewPanel {
  panelMode: 'View';
}
