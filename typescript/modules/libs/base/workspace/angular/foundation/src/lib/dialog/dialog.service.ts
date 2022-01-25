import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { DialogConfig } from './dialog.config';

@Injectable()
export abstract class AllorsDialogService {
  abstract alert(config: DialogConfig): Observable<any>;

  abstract confirm(config: DialogConfig): Observable<boolean>;

  abstract prompt(config: DialogConfig): Observable<string>;
}
