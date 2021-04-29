import { IWorkspace, IWorkspaceLifecycle } from '@allors/workspace/domain/system';
import { Class, MetaPopulation, RelationType } from '@allors/workspace/meta/system';
import { Database } from '../Database/Database';
import { WorkspaceObject } from './WorkspaceObject';

export class Workspace implements IWorkspace {
  private readonly objectById: Map<number, WorkspaceObject>;

  constructor(public name: string, public metaPopulation: MetaPopulation, instance: {}, state: IWorkspaceLifecycle, httpClient: HttpClient) {
    this.objectFactory = new ObjectFactory(this.metaPopulation, instance);
    this.database = new Database(this.metaPopulation, httpClient, new Identities());

    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.objectById = new Map();

    this.lifecycle.OnInit(this);
  }

  objectFactory: ObjectFactory;

  createSession(): ISession {
    return new Session(this, this.state.createSessionContext());
  }

  database: Database;

  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, number[]>;

  get(identity: number): WorkspaceObject {
    return this.objectById.get(identity);
  }

  registerWorkspaceObject(cls: IClass, workspaceId: number): void {
    this.workspaceClassByWorkspaceId.set(workspaceId, cls);

    let ids: Set<number> = this.workspaceIdsByWorkspaceClass.get(cls);
    if (!ids) {
      ids = [workspaceId];
    } else {
      ids = ids.add(ids, workspaceId);
    }

    this.workspaceIdsByWorkspaceClass[cls] = ids;
  }

  push(identity: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown>): void {
    let originalWorkspaceObject = this.objectById.get(identity);
    if (!originalWorkspaceObject) {
      this.objectById[identity] = new WorkspaceObject(this.database, identity, cls, ++version, changedRoleByRoleType);
    } else {
      this.objectById[identity] = new WorkspaceObject(originalWorkspaceObject, changedRoleByRoleType);
    }
  }
}
