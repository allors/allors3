import { IClass, IComposite, IInterface, InterfaceData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';
import { Composite } from './Composite';

export class Interface extends Composite implements IInterface {
  directSubtypes: IComposite[];
  private _classes: IClass[];

  constructor(metaPopulation: IMetaPopulationInternals, [tag, singularName]) {
    super(metaPopulation, tag, singularName);
  }

  init([, , , relationTypes, methodTypes, pluralName]: InterfaceData): void {
    super.init(relationTypes, methodTypes, pluralName);
  }

  get classes(): IClass[] {
    return this._classes ?? this.metaPopulation.classes.filter((v) => v.supertypes.includes(this));
  }

  isAssignableFrom(objectType: IComposite): boolean {
    throw new Error('Method not implemented.');
  }
}
