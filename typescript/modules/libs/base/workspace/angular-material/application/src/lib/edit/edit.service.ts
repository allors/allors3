import { Observable, throwError } from 'rxjs';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { Composite } from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';
import {
  EditData,
  EditService,
} from '@allors/base/workspace/angular/application';

@Injectable()
export class AllorsMaterialEditService extends EditService {
  editControlByObjectTypeTag: { [id: string]: any };

  constructor(public dialog: MatDialog) {
    super();
  }

  canEdit(object: IObject): boolean {
    return !!this.editControlByObjectTypeTag[object.strategy.cls.tag];
  }

  edit(object: IObject, objectType?: Composite): Observable<IObject> {
    const data: EditData = {
      kind: 'EditDialogData',
      object,
      objectType,
    };

    const component = this.editControlByObjectTypeTag[object.strategy.cls.tag];
    if (component) {
      const dialogRef = this.dialog.open(component, {
        data,
        minWidth: '80vw',
        maxHeight: '90vh',
      });
      return dialogRef.afterClosed();
    }

    return throwError(() => new Error('Missing component'));
  }
}
