import { Path } from '@allors/system/workspace/domain';
import { RoleType } from '@allors/system/workspace/meta';

export interface ObjectPanel {
  anchor: RoleType;

  target: Path;
}
