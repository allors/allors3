import { IConfiguration } from './IConfiguration';
import { IWorkspace } from './IWorkspace';

export interface IDatabase {
  configuration: IConfiguration;

  createWorkspace(): IWorkspace;
}
