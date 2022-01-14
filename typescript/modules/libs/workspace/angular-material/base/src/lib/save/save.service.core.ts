import { SaveService } from '@allors/workspace/angular/base';
import { ResultError } from '@allors/workspace/domain/system';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AllorsMaterialErrorDialogComponent } from './error/error-dialog.component';

@Injectable()
export class SaveServiceCore extends SaveService {
  public errorHandler: (error: any) => void;

  constructor(private dialog: MatDialog) {
    super();

    this.errorHandler = (error) => {
      if (error instanceof ResultError) {
        this.dialog.open(AllorsMaterialErrorDialogComponent, {
          data: { error },
          maxHeight: '90vh',
        });
      } else {
        window.alert(error.error ?? error.message ?? error);
        throw error;
      }
    };
  }
}
