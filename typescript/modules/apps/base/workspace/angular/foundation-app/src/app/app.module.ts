import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';

import {
  ThrottledConfig,
  ThrottledDirective,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { M } from '@allors/default/workspace/meta';

import { AngularClient } from '../allors/angular-client';
import { environment } from '../environments/environment';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { QueryComponent } from './query/query.component';
import { FetchComponent } from './fetch/fetch.component';
import { CoreContext } from '../allors/core-context';
import { Configuration } from '@allors/system/workspace/domain';
import { ruleBuilder } from '@allors/base/workspace/derivations-custom';

export function appInitFactory(
  workspaceService: WorkspaceService,
  httpClient: HttpClient
) {
  return async () => {
    const angularClient = new AngularClient(
      httpClient,
      environment.baseUrl,
      environment.authUrl
    );

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

    workspaceService.contextBuilder = () => new CoreContext(workspaceService);
  };
}

const routes: Routes = [
  {
    path: '',
    children: [
      {
        component: HomeComponent,
        path: '',
      },
      {
        component: QueryComponent,
        path: 'query',
      },
      {
        component: FetchComponent,
        path: 'fetch/:id',
      },
    ],
  },
];

@NgModule({
  declarations: [
    ThrottledDirective,
    AppComponent,
    HomeComponent,
    QueryComponent,
    FetchComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot(routes, { initialNavigation: 'enabledBlocking' }),
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient],
      multi: true,
    },
    {
      provide: ThrottledConfig,
      useValue: { time: 5000 },
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
