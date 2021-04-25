import { Procedure, Pull } from '@allors/data/core';

export class PullRequest {
  public pulls?: Pull[];

  public procedure?: Procedure;

  constructor(fields?: Partial<PullRequest>) {
    Object.assign(this, fields);
  }

  public toJSON() {
    return {
      p: this.pulls,
      s: this.procedure,
    };
  }
}
