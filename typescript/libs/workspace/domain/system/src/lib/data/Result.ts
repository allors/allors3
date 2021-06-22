import { Select } from "./Select";
import { IVisitable } from './visitor/IVisitable';

export interface Result extends IVisitable {
  selectRef?: string;

  select?: Select;

  name?: string;

  skip?: number;

  take?: number;
}
