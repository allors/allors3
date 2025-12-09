import { Configuration } from './configuration';
import { ISession } from './isession';

export interface IWorkspace {
  configuration: Configuration;

  createSession(): ISession;
}
