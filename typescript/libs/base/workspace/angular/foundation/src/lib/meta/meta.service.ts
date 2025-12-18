import { Composite, PropertyType } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export abstract class MetaService {
  abstract singularName(metaObject: Composite | PropertyType): string;

  abstract pluralName(metaObject: Composite | PropertyType): string;
}
