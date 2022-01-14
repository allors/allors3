import { Injectable } from '@angular/core';
import { RoleType } from '@allors/workspace/meta/system';
import { ObjectService, RefreshService } from '@allors/workspace/angular/base';
import { EditAction } from './edit-action';

@Injectable({
  providedIn: 'root',
})
export class EditService {
  constructor(
    private objectService: ObjectService,
    private refreshService: RefreshService
  ) {}

  edit(roleType?: RoleType) {
    return new EditAction(this.objectService, this.refreshService, roleType);
  }
}
