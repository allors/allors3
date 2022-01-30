import { Panel } from './panel';

export interface PanelManager {
  register(panel: Panel): void;

  unregister(panel: Panel): void;

  toggle(panel: Panel): void;
}
