import { ajax } from 'rxjs/ajax';
import { data, M } from '@allors/workspace/meta/core';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { AjaxClient } from '@allors/workspace/domain/json/rxjs/system';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  m: M;
  client: AjaxClient;

  async init(population = 'full') {
    await ajax(`${BASE_URL}Test/Setup?population=${population}`).toPromise();

    this.client = new AjaxClient(BASE_URL, AUTH_URL);

    this.m = (new LazyMetaPopulation(data) as unknown) as M;
  }

  async login(user = 'administrator', client: AjaxClient = this.client): Promise<boolean> {
    return client.login(user).toPromise();
  }
}
