import { ISession, IWorkspace, IWorkspaceLifecycle } from '@allors/workspace/domain/system';
import { Class, MetaPopulation, RelationType } from '@allors/workspace/meta/system';
import { Client } from '../Database/Client';
import { Database } from '../Database/Database';
import { Session } from '../Session/Session';
import { Identities } from '../Identities';
import { ObjectFactory } from '../ObjectFactory';
import { WorkspaceObject } from './WorkspaceObject';

export class Workspace implements IWorkspace {
  private readonly objectById: Map<number, WorkspaceObject>;

  objectFactory: ObjectFactory;

  constructor(public name: string, public metaPopulation: MetaPopulation, public lifecycle: IWorkspaceLifecycle, private client: Client) {
    this.objectFactory = new ObjectFactory(this.metaPopulation);
    this.database = new Database(this.metaPopulation, client, new Identities());

    this.workspaceClassByWorkspaceId = new Map();
    this.workspaceIdsByWorkspaceClass = new Map();

    this.objectById = new Map();

    this.lifecycle.onInit(this);
  }

  createSession(): ISession {
    return new Session(this, this.lifecycle.createSessionContext());
  }

  database: Database;

  workspaceClassByWorkspaceId: Map<number, Class>;

  workspaceIdsByWorkspaceClass: Map<Class, number[]>;

  get(identity: number): WorkspaceObject {
    return this.objectById.get(identity);
  }

  registerWorkspaceObject(cls: Class, workspaceId: number): void {
    // TODO:
    // this.workspaceClassByWorkspaceId.set(workspaceId, cls);

    // let ids: number[] = this.workspaceIdsByWorkspaceClass.get(cls);
    // if (!ids) {
    //   ids = [workspaceId];
    // } else {
    //   ids = ids.add(ids, workspaceId);
    // }

    // this.workspaceIdsByWorkspaceClass[cls] = ids;
  }

  push(identity: number, cls: Class, version: number, changedRoleByRoleType: Map<RelationType, unknown>): void {
    // TODO:
    // const originalWorkspaceObject = this.objectById.get(identity);
    // if (!originalWorkspaceObject) {
    //   this.objectById[identity] = new WorkspaceObject(this.database, identity, cls, ++version, changedRoleByRoleType);
    // } else {
    //   this.objectById[identity] = new WorkspaceObject(originalWorkspaceObject, changedRoleByRoleType);
    // }
  }
}
