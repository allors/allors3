import { Injectable } from '@angular/core';
import { RoleType } from '@allors/workspace/meta/system';
import { EditService, RefreshService } from '@allors/workspace/angular/base';
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
