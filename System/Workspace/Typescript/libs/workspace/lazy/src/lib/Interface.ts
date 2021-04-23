import { IClass, IComposite, IInterface, ObjectTypeData } from '@allors/workspace/system';
import { Composite } from './Composite';
import { MetaPopulation } from './MetaPopulation';

export class Interface extends Composite implements IInterface {
  private _classes: IClass[];

  private _subtypes: IComposite[];

  constructor(metaPopulation: MetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
  }
  get classes(): IClass[] {
    return this._classes ??= this.metaPopulation.classes.filter((v) => v.supertypes.includes(this));
  }

  isAssignableFrom(objectType: IComposite): boolean {
    return this === objectType || this.subtypes.includes(objectType);
  }

  get subtypes(): IComposite[] {
    return this._subtypes ??= this.metaPopulation.composites.filter(v=>v.supertypes.includes(this));
  }
}
