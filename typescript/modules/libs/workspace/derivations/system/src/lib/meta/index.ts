import { IObject, IRule } from '@allors/workspace/domain/system';
import { Class } from '@allors/workspace/meta/system';

declare module '@allors/workspace/meta/system' {
  interface MetaPopulationExtension {
    rules?: IRule<IObject>[];
  }

  interface RoleTypeExtension {
    rule?: IRule<IObject>;
    ruleByClass?: Map<Class, IRule<IObject>>;
  }
}
