import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { WorkspaceService } from '@allors/workspace/angular/core';
import { Configuration, IdGenerator, PrototypeObjectFactory, ServicesBuilder } from '@allors/workspace/adapters/system';
import { DatabaseConnection, ReactiveDatabaseClient } from '@allors/workspace/adapters/json/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { M } from '@allors/workspace/meta/default';
import { ruleBuilder } from '@allors/workspace/derivations/core-custom';

import { AngularClient } from '../allors/angular-client';
import { WorkspaceServices } from '../allors/workspace-services';
import { environment } from '../environments/environment';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { QueryComponent } from './query/query.component';
import { FetchComponent } from './fetch/fetch.component';

export function appInitFactory(workspaceService: WorkspaceService, httpClient: HttpClient) {
  return async () => {
    const client = new AngularClient(httpClient, environment.baseUrl, environment.authUrl);
    await client.login('jane@example.com', '');
    workspaceService.client = new ReactiveDatabaseClient(client);

    const metaPopulation = new LazyMetaPopulation(data);
    const configuration = new Configuration('Default', metaPopulation, new PrototypeObjectFactory(metaPopulation));
    let nextId = -1;
    const idGenerator: IdGenerator = () => nextId--;
    const serviceBuilder: ServicesBuilder = () => {
      return new WorkspaceServices(ruleBuilder(metaPopulation as any as M));
    };
    const database = new DatabaseConnection(configuration, idGenerator, serviceBuilder);
    workspaceService.workspace = database.createWorkspace();
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
