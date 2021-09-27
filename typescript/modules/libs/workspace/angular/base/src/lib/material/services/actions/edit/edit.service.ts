import { Injectable } from '@angular/core';

import { RoleType } from '@allors/workspace/meta/system';

import { ObjectService } from '../../object/object.service';
import { RefreshService } from '../../../../services/refresh/refresh.service';

import { EditAction } from './edit-action';

@Injectable({
  providedIn: 'root',
})
export class EditService {

  constructor(
    private objectService: ObjectService,
    private refreshService: RefreshService,
    ) {}

  edit(roleType?: RoleType) {
    return new EditAction(this.objectService, this.refreshService, roleType);
  }

}
