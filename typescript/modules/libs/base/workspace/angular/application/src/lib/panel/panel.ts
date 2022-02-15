export type PanelMode = 'Edit' | 'View';

export type PanelKind =
  | 'Summary'
  | 'Detail'
  | 'Relation'
  | 'Relationship'
  | 'Object';

export interface Panel {
  panelMode: PanelMode;

  panelKind: PanelKind;

  panelId: string;

  panelEnabled: boolean;
}
