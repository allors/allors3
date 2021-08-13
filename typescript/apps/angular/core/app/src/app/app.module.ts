import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { WorkspaceService } from '@allors/workspace/angular/core';
import { Configuration, PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/core';

import { FetchClient } from '../allors/FetchClient';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { QueryComponent } from './query/query.component';
import { FetchComponent } from './fetch/fetch.component';
import { environment } from '../environments/environment';

export function appInitFactory(workspaceService: WorkspaceService) {
  return async () => {
    const client = new FetchClient(environment.baseUrl, environment.authUrl);
    await client.login('administrator', '');

    const metaPopulation = new LazyMetaPopulation(data);
    let nextId = -1;
    const database = new Database(
      new Configuration('Default', metaPopulation, new PrototypeObjectFactory(metaPopulation)),
      () => nextId--,
      () => {
        return new WorkspaceServices([]);
      },
      client
    );

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
  imports: [BrowserModule, FormsModule, RouterModule.forRoot(routes, { initialNavigation: 'enabledBlocking' })],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService],
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
