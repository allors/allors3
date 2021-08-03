import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';

import { AllorsMaterialDialogComponent } from '../../components/dialog/dialog.component';

import { AllorsMaterialDialogService } from './dialog.service';
import { DialogData } from './dialog.data';
import { DialogConfig } from './dialog.config';

@Injectable()
export class AllorsMaterialDialogServiceCore extends AllorsMaterialDialogService {
  constructor(private dialog: MatDialog) {
    super();
  }

  public alert(config: DialogConfig): Observable<any> {
    const data: DialogData = {
      alert: true,
      config,
    };

    data.config.title = data.config.title || 'Alert';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, { data, maxHeight: '90vh' });
    return dialogRef.afterClosed();
  }

  public confirm(config: DialogConfig): Observable<boolean> {
    const data: DialogData = {
      confirmation: true,
      config,
    };

    data.config.title = data.config.title || 'Confirm';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, { data, maxHeight: '90vh' });
    return dialogRef.afterClosed();
  }

  public prompt(config: DialogConfig): Observable<string> {
    const data: DialogData = {
      prompt: true,
      config,
    };

    data.config.title = data.config.title || 'Prompt';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, { data, maxHeight: '90vh' });
    return dialogRef.afterClosed();
  }
}
