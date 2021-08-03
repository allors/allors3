import { Injectable, Inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable, throwError } from 'rxjs';

import { IObject } from '@allors/workspace/domain/system';
import { ObjectType } from '@allors/workspace/meta/system';

import { Context } from '../../../services/framework/Context';

import { ObjectService } from './object.service';
import { OBJECT_CREATE_TOKEN, OBJECT_EDIT_TOKEN } from './object.tokens';
import { ObjectData } from './object.data';

@Injectable()
export class ObjectServiceCore extends ObjectService {

  constructor(
    public dialog: MatDialog,
    @Inject(OBJECT_CREATE_TOKEN) private createControlByObjectTypeId: { [id: string]: any },
    @Inject(OBJECT_EDIT_TOKEN) private editControlByObjectTypeId: { [id: string]: any }
  ) {
    super();
  }

  create(objectType: ObjectType, createData?: ObjectData): Observable<IObject> {

    const data: ObjectData = Object.assign({ objectType }, createData);

    const component = this.createControlByObjectTypeId[objectType.id];
    if (component) {
      const dialogRef = this.dialog.open(component, { data, minWidth: '80vw', maxHeight: '90vh' });

      return dialogRef
        .afterClosed();
    }

    return throwError('Missing component');
  }

  hasCreateControl(objectType: ObjectType, createData: ObjectData, context: Context) {
    const createControl = this.createControlByObjectTypeId[objectType.id];
    if (createControl) {
      if (createControl.canCreate) {
        return createControl.canCreate(createData, context);
      }

      return true;
    }

    return false;
  }

  edit(object: IObject, createData?: ObjectData): Observable<IObject> {

    const data: ObjectData = Object.assign({
      id: object.id,
      objectType: object.objectType,
    }, createData);

    const component = this.editControlByObjectTypeId[object.objectType.id];
    if (component) {
      const dialogRef = this.dialog.open(component, { data, minWidth: '80vw', maxHeight: '90vh' });
      return dialogRef.afterClosed();
    }

    return throwError('Missing component');
  }

  hasEditControl(object: IObject) {
    return !!this.editControlByObjectTypeId[object.objectType.id];
  }

}
