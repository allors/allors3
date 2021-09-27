import { IConfiguration } from './iconfiguration';
import { ISession } from './isession';
import { IWorkspaceServices } from './services/iworkspace-services';

export interface IWorkspace {

  configuration: IConfiguration;

  services: IWorkspaceServices;

  createSession(): ISession;
}
