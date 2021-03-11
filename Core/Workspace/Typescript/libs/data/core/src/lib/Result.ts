import { Select } from './Select';

export class Result {
  public selectRef?: string;

  public select?: Select;

  public name?: string;

  public skip?: number;

  public take?: number;

  constructor(args?: Partial<Result>) {
    if (args) {
      Object.assign(this, args);
    }
  }
}
