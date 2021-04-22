import { IClass, IComposite, IInterface, ObjectTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Interface extends Composite implements IInterface {
  directSubtypes: IComposite[];
  private _classes: IClass[];

  constructor(metaPopulation: IMetaPopulationInternals, [tag, singularName, r, m, pluralName]: ObjectTypeData) {
    super(metaPopulation, tag, singularName, r, m, pluralName);
  }

  get classes(): IClass[] {
    return this._classes ?? this.metaPopulation.classes.filter((v) => v.supertypes.includes(this));
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
