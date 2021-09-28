import { Fixture } from '../../fixture';
import { Context } from './context';

export class SingleSessionContext extends Context {
  constructor(fixture: Fixture, name: string) {
    super(fixture, name);
    this.session1 = this.fixture.workspace.createSession();
    this.session2 = this.session1;
  }
}
