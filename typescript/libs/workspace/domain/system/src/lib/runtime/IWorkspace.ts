import { IDatabase } from './IDatabase';
import { ISession } from './ISession';
import { IWorkspaceServices } from './IWorkspaceServices';

export interface IWorkspace {

  database: IDatabase;

  services: IWorkspaceServices;

  createSession(): ISession;
}
