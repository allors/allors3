import { Injectable, Inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, throwError } from 'rxjs';

import { IObject, ISession } from '@allors/workspace/domain/system';
import { ObjectType } from '@allors/workspace/meta/system';

import { ObjectService } from './object.service';
import { OBJECT_CREATE_TOKEN, OBJECT_EDIT_TOKEN } from './object.tokens';
import { ObjectData } from './object.data';

@Injectable()
export class ObjectServiceCore extends ObjectService {
  constructor(public dialog: MatDialog, @Inject(OBJECT_CREATE_TOKEN) private createControlByObjectTypeTag: { [id: string]: any }, @Inject(OBJECT_EDIT_TOKEN) private editControlByObjectTypeTag: { [id: string]: any }) {
    super();
  }

  create(objectType: ObjectType, createData?: ObjectData): Observable<IObject> {
    const data: ObjectData = Object.assign({ strategy: { cls: objectType } }, createData);

    const component = this.createControlByObjectTypeTag[objectType.tag];
    if (component) {
      const dialogRef = this.dialog.open(component, { data, minWidth: '80vw', maxHeight: '90vh' });

      return dialogRef.afterClosed();
    }

    return throwError('Missing component');
  }

  hasCreateControl(objectType: ObjectType, createData: ObjectData, session: ISession) {
    const createControl = this.createControlByObjectTypeTag[objectType.tag];
    if (createControl) {
      if (createControl.canCreate) {
        return createControl.canCreate(createData, session);
      }

      return true;
    }

    return false;
  }

  edit(object: IObject, createData?: ObjectData): Observable<IObject> {
    const data: ObjectData = Object.assign(
      {
        id: object.id,
        objectType: object.strategy.cls.tag,
      },
      createData
    );

    const component = this.editControlByObjectTypeTag[object.strategy.cls.tag];
    if (component) {
      const dialogRef = this.dialog.open(component, { data, minWidth: '80vw', maxHeight: '90vh' });
      return dialogRef.afterClosed();
    }

    return throwError('Missing component');
  }

  hasEditControl(object: IObject) {
    return !!this.editControlByObjectTypeTag[object.strategy.cls.tag];
  }
}
