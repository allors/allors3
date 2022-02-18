import { Path } from '@allors/system/workspace/domain';
import { PropertyType, RoleType } from '@allors/system/workspace/meta';

export interface ObjectPanel {
  anchor: PropertyType | PropertyType[];

  target: PropertyType | Path | (PropertyType | Path)[];
}
