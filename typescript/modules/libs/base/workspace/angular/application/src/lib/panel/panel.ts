export type PanelKind = 'Summary' | 'Detail' | 'RelationShip';

export type PanelMode = 'View' | 'Edit';

export interface Panel {
  panelId: string;

  panelKind: PanelKind;

  panelMode: PanelMode;
}
