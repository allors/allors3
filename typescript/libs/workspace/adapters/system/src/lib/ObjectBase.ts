import { IObject, IStrategy } from '@allors/workspace/domain/system';

export abstract class ObjectBase implements IObject {
  get id(): number {
    return this.strategy.id;
  }

  strategy: IStrategy;

  init() {}
}
