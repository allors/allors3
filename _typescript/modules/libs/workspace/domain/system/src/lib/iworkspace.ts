import { IConfiguration } from './IConfiguration';
import { ISession } from './ISession';
import { IWorkspaceServices } from './services/IWorkspaceServices';

export interface IWorkspace {

  configuration: IConfiguration;

  services: IWorkspaceServices;

  createSession(): ISession;
}
