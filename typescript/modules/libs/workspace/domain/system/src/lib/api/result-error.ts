import { IResult } from './iresult';

export class ResultError extends Error {
  constructor(public result: IResult) {
    super();

    // Fix for extending builtin objects for es5
    Object.setPrototypeOf(this, ResultError.prototype);
  }
}
