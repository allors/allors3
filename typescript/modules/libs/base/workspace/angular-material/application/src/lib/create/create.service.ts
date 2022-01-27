import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, throwError } from 'rxjs';
import { IObject } from '@allors/system/workspace/domain';
import { Composite } from '@allors/system/workspace/meta';
import {
  CreateRequest,
  CreateService,
} from '@allors/base/workspace/angular/foundation';

@Injectable()
export class AllorsMaterialCreateService extends CreateService {
  createControlByObjectTypeTag: { [id: string]: any };

  constructor(public dialog: MatDialog) {
    super();
  }

  canCreate(objectType: Composite): boolean {
    return !!this.createControlByObjectTypeTag[objectType.tag];
  }

  create(request: CreateRequest): Observable<IObject> {
    const component = this.createControlByObjectTypeTag[request.objectType.tag];
    if (component) {
      const dialogRef = this.dialog.open(component, {
        data: request,
        minWidth: '80vw',
        maxHeight: '90vh',
      });

      return dialogRef.afterClosed();
    }

    return throwError(() => new Error('Missing component'));
  }
}
