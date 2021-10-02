import { IConfiguration } from './iconfiguration';
import { ISession } from './isession';

export interface IWorkspace {
  configuration: IConfiguration;

  createSession(): ISession;
}
