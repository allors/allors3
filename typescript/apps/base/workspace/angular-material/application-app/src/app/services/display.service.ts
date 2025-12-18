import { Injectable } from '@angular/core';
import { IObject } from '@allors/system/workspace/domain';
import { Class, Composite, RoleType } from '@allors/system/workspace/meta';
import {
  DisplayService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

@Injectable()
export class AppDisplayService implements DisplayService {
  nameByObjectType: Map<Composite, RoleType>;
  descriptionByObjectType: Map<Composite, RoleType>;
  primaryByObjectType: Map<Composite, RoleType[]>;
  secondaryByObjectType: Map<Composite, RoleType[]>;
  tertiaryByObjectType: Map<Composite, RoleType[]>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.nameByObjectType = new Map<Composite, RoleType>([
      [m.HomeAddress, m.HomeAddress.Street],
      [m.Organisation, m.Organisation.Name],
      [m.Person, m.Person.DomainFullName],
    ]);

    this.descriptionByObjectType = new Map<Composite, RoleType>([]);

    this.primaryByObjectType = new Map<Composite, RoleType[]>([
      [m.Person, [m.Person.FirstName, m.Person.LastName, m.Person.Address]],
      [
        m.Organisation,
        [
          m.Organisation.Name,
          m.Organisation.Owner,
          m.Organisation.IncorporationDate,
        ],
      ],
    ]);

    this.secondaryByObjectType = new Map<Composite, RoleType[]>([]);

    this.tertiaryByObjectType = new Map<Composite, RoleType[]>([]);
  }

  name(objectType: Composite): RoleType {
    return this.nameByObjectType.get(objectType);
  }

  description(objectType: Composite): RoleType {
    return this.nameByObjectType.get(objectType);
  }

  primary(objectType: Composite): RoleType[] {
    return this.primaryByObjectType.get(objectType) ?? [];
  }

  secondary(objectType: Composite): RoleType[] {
    return this.secondaryByObjectType.get(objectType) ?? [];
  }

  tertiary(objectType: Composite): RoleType[] {
    return this.tertiaryByObjectType.get(objectType) ?? [];
  }
}
