import { IRule } from './derivation/rules/irule';
import { IConfiguration } from './iconfiguration';
import { ISession } from './isession';

export interface IWorkspace {
  configuration: IConfiguration;

  rules: IRule[];

  rule(cls: new (...args: any[]) => IRule): IRule;

  createSession(): ISession;
}
