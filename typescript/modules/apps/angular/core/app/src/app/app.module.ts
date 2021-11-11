import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { WorkspaceService } from '@allors/workspace/angular/core';
import { PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { M } from '@allors/workspace/meta/default';
import { ruleBuilder } from '@allors/workspace/derivations/core-custom';

import { AngularClient } from '../allors/angular-client';
import { environment } from '../environments/environment';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { QueryComponent } from './query/query.component';
import { FetchComponent } from './fetch/fetch.component';
import { CoreContext } from '../allors/core-context';
import { Configuration } from '@allors/workspace/domain/system';

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    const angularClient = new AngularClient(httpClient, environment.baseUrl, environment.authUrl);

    const metaPopulation = new LazyMetaPopulation(data);
    const m = metaPopulation as unknown as M;

    let nextId = -1;

    const configuration: Configuration = {
      name: 'Default',
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
      rules: ruleBuilder(m),
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
  declarations: [AppComponent, HomeComponent, QueryComponent, FetchComponent],
  imports: [BrowserModule, FormsModule, HttpClientModule, RouterModule.forRoot(routes, { initialNavigation: 'enabledBlocking' })],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, HttpClient],
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
