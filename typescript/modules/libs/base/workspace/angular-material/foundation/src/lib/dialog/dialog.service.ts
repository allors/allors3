import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { AllorsMaterialDialogData } from './dialog.data';
import { AllorsMaterialDialogComponent } from './dialog.component';
import {
  AllorsDialogService,
  DialogConfig,
} from '@allors/base/workspace/angular/foundation';

@Injectable()
export class AllorsMaterialDialogService extends AllorsDialogService {
  constructor(private dialog: MatDialog) {
    super();
  }

  public alert(config: DialogConfig): Observable<any> {
    const data: AllorsMaterialDialogData = {
      alert: true,
      config,
    };

    data.config.title = data.config.title || 'Alert';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, {
      data,
      maxHeight: '90vh',
    });
    return dialogRef.afterClosed();
  }

  public confirm(config: DialogConfig): Observable<boolean> {
    const data: AllorsMaterialDialogData = {
      confirmation: true,
      config,
    };

    data.config.title = data.config.title || 'Confirm';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, {
      data,
      maxHeight: '90vh',
    });
    return dialogRef.afterClosed();
  }

  public prompt(config: DialogConfig): Observable<string> {
    const data: AllorsMaterialDialogData = {
      prompt: true,
      config,
    };

    data.config.title = data.config.title || 'Prompt';

    const dialogRef = this.dialog.open(AllorsMaterialDialogComponent, {
      data,
      maxHeight: '90vh',
    });
    return dialogRef.afterClosed();
  }
}
