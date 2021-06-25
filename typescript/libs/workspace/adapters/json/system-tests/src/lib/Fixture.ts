import { MetaPopulation } from '@allors/workspace/meta/system';
import { M } from '@allors/workspace/meta/core';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/core';
import { FetchClient } from './FetchClient';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  metaPopulation: MetaPopulation;
  m: M;
  client: FetchClient;

  async init(population?: string) {
    this.client = new FetchClient(BASE_URL, AUTH_URL);

    await this.client.setup(population);
    await this.client.login('administrator', '');

    this.metaPopulation = new LazyMetaPopulation(data);
    this.m = (this.metaPopulation as MetaPopulation) as M;
  }
}
