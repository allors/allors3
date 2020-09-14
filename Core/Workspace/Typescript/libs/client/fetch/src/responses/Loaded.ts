import { DatabaseObject, Session, WorkspaceObject } from '@allors/workspace/system';
import { PullResponse } from '@allors/protocol/system';
import { assert } from '@allors/meta/system';

export class Loaded {
  public objects: { [name: string]: DatabaseObject | WorkspaceObject } = {};
  public collections: { [name: string]: (DatabaseObject| WorkspaceObject)[] } = {};
  public values: { [name: string]: any } = {};

  constructor(public session: Session, public response: PullResponse) {
    const namedObjects = response.namedObjects;
    const namedCollections = response.namedCollections;
    const namedValues = response.namedValues;

    if (namedObjects) {
      Object.keys(namedObjects).map((key: string) => {
        const object = session.get(namedObjects[key]);
        assert(object);
        this.objects[key] = object;
      });
    }

    if (namedCollections) {
      Object.keys(namedCollections).map((key: string) => {
        const collection = namedCollections[key].map((id: string) => {
          const object = session.get(id);
          assert(object);
          return object;
        });

        this.collections[key] = collection;
      });
    }

    if (namedValues) {
      Object.keys(namedValues).map((key: string) => (this.values[key] = namedValues[key]));
    }
  }
}
