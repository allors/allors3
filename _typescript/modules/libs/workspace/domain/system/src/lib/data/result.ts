import { Select } from './Select';
import { Node } from "./Node";

export interface Result {
  selectRef?: string;

  select?: Select;

  include?: Node[];

  name?: string;

  skip?: number;

  take?: number;
}
