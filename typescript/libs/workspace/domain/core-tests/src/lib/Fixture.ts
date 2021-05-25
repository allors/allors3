import { map, tap } from 'rxjs/operators';
import { ajax } from 'rxjs/ajax';
import { data, M } from '@allors/workspace/meta/core';
import { LazyMetaPopulation } from '@allors/workspace/meta/lazy/system';
import { Client, AjaxClient } from '@allors/workspace/adapters/json/system';
import { AuthenticationTokenRequest, AuthenticationTokenResponse } from '@allors/protocol/json/system';

export class Fixture {
  m: M;
  client: Client;

  async init(population?: string) {
    this.m = (new LazyMetaPopulation(data) as unknown) as M;
    this.client = new AjaxClient();

    await ajax.get(`http://localhost:5000/allors/Test/Setup?population=${population ?? 'full'}`).toPromise();
    if (!(await this.login())) {
      throw new Error('Could not log in');
    }
  }

  async login(user = 'administrator', client: Client = this.client): Promise<boolean> {
    const request: AuthenticationTokenRequest = {
      l: user,
      p: null,
    };

    return await ajax
      .post(`http://localhost:5000/allors/TestAuthentication/Token`, request, { 'Content-Type': 'application/json' })
      .pipe(
        map((ajax) => {
          return ajax.response as AuthenticationTokenResponse;
        }),
        tap((result) => {
          if (result.a) {
            client.userId = result.u;
            client.jwtToken = result.t;
          }
        }),
        map((result) => result.a)
      )
      .toPromise();
  }
}
