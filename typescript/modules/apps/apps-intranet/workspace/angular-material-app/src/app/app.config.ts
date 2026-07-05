import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  SingletonId,
  UserId,
  UserInfoService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';

import { AppClient } from './app.client';
import { Configuration } from '@allors/system/workspace/domain';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { data } from '@allors/default/workspace/meta-json';
import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  Person,
  Singleton,
} from '@allors/default/workspace/domain';
import { ruleBuilder } from '@allors/default/workspace/derivations';
import { AppContext } from './app.context';

export function config(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  baseUrl: string
) {
  const angularClient = new AppClient(httpClient, baseUrl);

  const metaPopulation = new LazyMetaPopulation(data);
  const m = metaPopulation as unknown as M;

  let nextId = -1;

  const configuration: Configuration = {
    name: 'Default',
    metaPopulation,
    objectFactory: new PrototypeObjectFactory(metaPopulation),
    rules: ruleBuilder(m),
    idGenerator: () => nextId--,
  };

  const database = new DatabaseConnection(configuration, angularClient);
  const workspace = database.createWorkspace();
  workspaceService.workspace = workspace;

  workspaceService.contextBuilder = () => new AppContext(workspaceService);
}

// APP_INITIALIZER body shared by the dev/prod environments: configures the workspace, learns the
// logged-in user from the cookie-authenticated /allors/UserInfo endpoint, then runs the bootstrap
// pull that used to live in the login component (default internal organisation + singleton). A 401
// from UserInfo redirects to the login page via the UnauthorizedInterceptor before we reach here.
export async function initialize(
  workspaceService: WorkspaceService,
  httpClient: HttpClient,
  baseUrl: string,
  userInfoService: UserInfoService,
  userId: UserId,
  internalOrganisationId: InternalOrganisationId,
  singletonId: SingletonId
): Promise<void> {
  config(workspaceService, httpClient, baseUrl);

  await userInfoService.init(baseUrl);

  const id = userId.value;
  if (id == null) {
    return;
  }

  const context = workspaceService.contextBuilder();
  const m = context.configuration.metaPopulation as unknown as M;
  const { pullBuilder: p } = m;

  const pulls = [
    p.Person({
      objectId: id,
      include: {
        UserProfile: {
          DefaultInternalOrganization: {},
        },
      },
    }),
    p.Organisation({
      predicate: {
        kind: 'Equals',
        propertyType: m.Organisation.IsInternalOrganisation,
        value: true,
      },
    }),
    p.Singleton({}),
  ];

  const loaded = await firstValueFrom(context.pull(pulls));

  const person = loaded.object<Person>(m.Person);
  const internalOrganisations = loaded.collection<Organisation>(m.Organisation);
  const internalOrganisation =
    person?.UserProfile?.DefaultInternalOrganization ??
    internalOrganisations[0];
  internalOrganisationId.value = internalOrganisation?.strategy.id;

  const singleton = loaded.collection<Singleton>(m.Singleton)[0];
  singletonId.value = singleton?.strategy.id;
}
