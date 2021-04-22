import { IClass, IComposite, IInterface, InterfaceData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Interface extends Composite implements IInterface {
  directSubtypes: IComposite[];
  private _classes: IClass[];

  constructor(metaPopulation: IMetaPopulationInternals, [tag, singularName, , , ,pluralName] : InterfaceData) {
    super(metaPopulation, tag, singularName, pluralName);
  }

  get classes(): IClass[] {
    return this._classes ?? this.metaPopulation.classes.filter((v) => v.supertypes.includes(this));
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
