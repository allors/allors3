import { MetaPopulation, assert, MetaObject, RoleType, RelationType, AssociationType } from '@allors/meta/core';
import { PushRequest } from '@allors/protocol/json/system';

export class PushRequestInfo {
  metaTypeByKey: Map<string, MetaObject>;

  constructor(pushRequest: PushRequest, meta: MetaPopulation) {
    this.metaTypeByKey = new Map();

    const keys: Set<string> = new Set();

    pushRequest.newObjects?.forEach(v => {
      keys.add(v.t);
      v.roles?.forEach(w => keys.add(w.t));
    });

    pushRequest.objects?.forEach(v => {
      v.roles?.forEach(w => keys.add(w.t));
    });

    keys.forEach(key => {
      const id = key;
      const objectType = meta.metaObjectById.get(id);
      assert(objectType);
      this.metaTypeByKey.set(key, objectType);
    });
  }

  is(key: string, metaObject: MetaObject): boolean {

    if(metaObject instanceof RoleType){
      return (this.metaTypeByKey.get(key) as RelationType).roleType === metaObject;
    }

    if(metaObject instanceof AssociationType){
      return (this.metaTypeByKey.get(key) as RelationType).associationType === metaObject;
    }

    return this.metaTypeByKey.get(key) === metaObject;
  }
}
