import { MetaPopulation } from '@allors/meta/system';
import { Meta, PullFactory, TreeFactory, FetchFactory, data } from '@allors/meta/generated';
import { Database } from '@allors/domain/system';
import { MemoryDatabase } from '@allors/domain/memory';

import '@allors/meta/core';
import { extend as extendDomain } from '@allors/domain/custom';

import { Context, AxiosHttp, Client } from '@allors/promise/core';

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

    await this.login('administrator');
    await this.http.get('Test/Setup', { population });
    const client = new Client(this.http);
    this.ctx = new Context(client, database);

    this.tree = new TreeFactory(this.m);
    this.fetch = new FetchFactory(this.m);
    this.pull = new PullFactory(this.m);

  }

  async login(user: string) {
    await this.http.login('TestAuthentication/Token', user);
  }
}
