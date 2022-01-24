import { IObject, IStrategy } from '@allors/system/workspace/domain';

export abstract class ObjectBase implements IObject {
  get id(): number {
    return this.strategy.id;
  }

  strategy: IStrategy;

  init() {}
}
