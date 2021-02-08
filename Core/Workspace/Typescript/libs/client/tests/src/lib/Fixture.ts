import { MetaPopulation } from '@allors/meta/core';
import { Meta, PullFactory, TreeFactory, FetchFactory, data } from '@allors/meta/generated';
import { Database } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/workspace/memory';

import { extend as extendDomain } from '@allors/domain/custom';

import { Context, AxiosHttp, Client } from '@allors/client/fetch';

export class Fixture {

  readonly FULL_POPULATION = 'full';

  metaPopulation: MetaPopulation;
  m: Meta;
  ctx: Context;

  pull: PullFactory;
  tree: TreeFactory;
  fetch: FetchFactory;

  readonly x = new Object();

  private http: AxiosHttp;

  async init(population?: string) {

    this.http = new AxiosHttp({ baseURL: 'http://localhost:5000/' });

    this.metaPopulation = new MetaPopulation(data);
    this.m = this.metaPopulation as Meta;
    const database: Database = new MemoryDatabase(this.metaPopulation);
    extendDomain(database);

    await this.http.get('Test/Setup', { population });
    const client = new Client(this.http);
    this.ctx = new Context(client, database);

    this.tree = new TreeFactory(this.m);
    this.fetch = new FetchFactory(this.m);
    this.pull = new PullFactory(this.m);

    await this.login('administrator');
  }

  async login(user: string) {
    await this.http.login('TestAuthentication/Token', user);
  }
}
