import { ErrorService } from '@allors/base/workspace/angular/foundation';
import { ResultError } from '@allors/system/workspace/domain';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AllorsMaterialErrorDialogComponent } from './error-dialog.component';

@Injectable()
export class AllorsMaterialErrorService extends ErrorService {
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
