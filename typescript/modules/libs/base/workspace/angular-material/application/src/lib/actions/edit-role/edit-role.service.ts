import { Injectable } from '@angular/core';
import { RoleType } from '@allors/system/workspace/meta';
import {
  RefreshService,
  EditService,
} from '@allors/base/workspace/angular/foundation';
import { EditRoleAction } from './edit-role-action';

@Injectable({
  providedIn: 'root',
})
export class EditRoleService {
  constructor(
    private objectService: EditService,
    private refreshService: RefreshService
  ) {}

  edit(roleType?: RoleType) {
    return new EditRoleAction(
      this.objectService,
      this.refreshService,
      roleType
    );
  }
}
