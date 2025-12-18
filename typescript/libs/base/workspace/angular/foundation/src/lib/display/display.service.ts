import { Composite, RoleType } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class DisplayService {
  abstract name(objectType: Composite): RoleType;

  abstract description(objectType: Composite): RoleType;

  abstract primary(objectType: Composite): RoleType[];

  abstract secondary(objectType: Composite): RoleType[];

  abstract tertiary(objectType: Composite): RoleType[];
}
