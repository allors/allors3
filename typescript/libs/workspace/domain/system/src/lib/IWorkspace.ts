import { MetaPopulation } from "@allors/workspace/meta/system";
import { IObjectFactory } from "./IObjectFactory";
import { ISession } from "./ISession";
import { IWorkspaceServices } from "./state/IWorkspaceServices";

export interface IWorkspace {
  name: string;

  metaPopulation: MetaPopulation;

  objectFactory: IObjectFactory;

  lifecycle: IWorkspaceServices;

  createSession(): ISession;
}
