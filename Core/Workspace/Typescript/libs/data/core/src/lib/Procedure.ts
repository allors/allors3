export type ProcedureArgs = Pick<Procedure, 'name' | 'namedCollections' | 'namedObjects' | 'namedValues' | 'objectVersions'>;

export class Procedure {
  name: string;

  namedCollections?: string[][];

  namedObjects?: string[][];

  namedValues?: string[][];

  objectVersions?: string[][];

  constructor(args: ProcedureArgs) {
    Object.assign(this, args);
  }

  public toJSON(): any {
    return {
      name: this.name,
      namedCollections: this.namedCollections,
      namedObjects: this.namedObjects,
      namedValues: this.namedValues,
      objectVersions: this.objectVersions,
    };
  }
}
