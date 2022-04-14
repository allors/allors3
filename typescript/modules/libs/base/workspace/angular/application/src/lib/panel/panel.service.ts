import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { Panel } from './panel';

@Injectable()
export abstract class PanelService {
  readonly activePanel: Panel;

  abstract register(panel: Panel): void;

  abstract unregister(panel: Panel): void;

  abstract startEdit(panelId: string): Observable<boolean>;

  abstract stopEdit(): Observable<boolean>;
}
