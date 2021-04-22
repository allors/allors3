import { IAssociationType, IClass, IComposite, IInterface, IMethodType, IRoleType, Origin, pluralize, ObjectTypeData, RelationTypeData, MethodTypeData } from '@allors/workspace/system';
import { IMetaPopulationInternals } from './Internals/IMetaPopulationInternals';

export abstract class Composite implements IComposite {
  directSupertypes: IInterface[];
  associationTypes: IAssociationType[];
  roleTypes: IRoleType[];
  methodTypes: IMethodType[];
  abstract classes: IClass[];

  private _supertypes: IInterface[];
  private _origin: Origin;

  constructor(public metaPopulation: IMetaPopulationInternals, public tag: number, public singularName: string, private r: RelationTypeData[], private m: MethodTypeData[], private _pluralName: string) {
    metaPopulation.onMetaObject(this);
  }

  get origin() {
    // return (this._origin ??= );
    return this._origin;
  }

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  get supertypes(): IInterface[] {
    return this._supertypes ?? this.directSupertypes.concat(...this.directSupertypes.map((v) => v.supertypes));
  }

  abstract isAssignableFrom(objectType: IComposite): boolean;

  init(): void {
  }
 
}
