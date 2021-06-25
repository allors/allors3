import { Database } from '@allors/workspace/adapters/system';
import { Tests } from '../Tests';

export class PullTests extends Tests {
  constructor(database: Database, public login: (login: string) => Promise<boolean>) {
    super(database, login);
  }

  async andGreaterThanLessThan() {
    await this.login('administrator');



    //     let session = this.Workspace.CreateSession();
    //     let m = this.M;
    //     //  Class
    //     let pull = [][
    //             Extent=newExtent(this.M.C1Unknown{Predicate=newAnd{Operands=newIPredicate[Unknown{newGreaterThan(m.C1.C1AllorsIntegerUnknown{Value=0Unknown,newLessThan(m.C1.C1AllorsIntegerUnknown{Value=2UnknownUnknownUnknownUnknown];
    //     let result = session.Pull(pull);
    //     Assert.Single(result.Collections);
    //     Assert.Empty(result.Objects);
    //     Assert.Empty(result.Values);
    //     result.Assert().Collection().Equal(c1B);
    //     //  Interface
    //     pull = [][
    //             Extent=newExtent(this.M.I12Unknown{Predicate=newAnd{Operands=newIPredicate[Unknown{newGreaterThan(m.I12.I12AllorsIntegerUnknown{Value=0Unknown,newLessThan(m.I12.I12AllorsIntegerUnknown{Value=2UnknownUnknownUnknownUnknown];
    //     result = session.Pull(pull);
    //     Assert.Single(result.Collections);
    //     Assert.Empty(result.Objects);
    //     Assert.Empty(result.Values);
    //     result.Assert().Collection().Equal(c1B, c2B);
  }
}
