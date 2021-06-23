import { IDatabase } from './IDatabase';
import { ISession } from './ISession';
import { IWorkspaceServices } from './state/IWorkspaceServices';

export interface IWorkspace {

  database: IDatabase;

  services: IWorkspaceServices;

  createSession(): ISession;
}
