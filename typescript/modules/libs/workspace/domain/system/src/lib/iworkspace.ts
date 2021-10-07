import { IRule } from './derivation/rules/irule';
import { IConfiguration } from './iconfiguration';
import { ISession } from './isession';

export interface IWorkspace {
  configuration: IConfiguration;

  rules: Readonly<IRule[]>;

  rule(cls: any): IRule;

  createSession(): ISession;
}
