import { MetaPopulation } from '@allors/meta/core';
import { Meta, PullFactory, TreeFactory, SelectFactory, data } from '@allors/meta/generated';
import { Database } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/adapters/memory/system';

import { extend as extendDomain } from '@allors/domain/custom';

import { Context, AxiosHttp, Client } from '@allors/client/fetch';

export class Fixture {

  readonly FULL_POPULATION = 'full';

  metaPopulation: MetaPopulation;
  m: Meta;
  ctx: Context;

  pull: PullFactory;
  tree: TreeFactory;
  select: SelectFactory;

  readonly x = new Object();

  private http: AxiosHttp;

  async init(population?: string) {

    this.http = new AxiosHttp({ baseURL: 'http://localhost:5000/allors/' });

    this.metaPopulation = new MetaPopulation(data);
    this.m = this.metaPopulation as Meta;
    const database: Database = new MemoryDatabase(this.metaPopulation);
    extendDomain(database);

    await this.http.get('Test/Setup', { population });
    const client = new Client(this.http);
    this.ctx = new Context(client, database);

    this.tree = new TreeFactory(this.m);
    this.select = new SelectFactory(this.m);
    this.pull = new PullFactory(this.m);

    await this.login('administrator');
  }

  async login(user: string) {
    await this.http.login('TestAuthentication/Token', user);
  }
}
