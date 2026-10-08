import { Node } from '../pointer/node';
import { Select } from './select';

export interface FlatResult {
  selectRef?: string;

  select?: Select | any;

  include?: Node[] | any;

  name?: string;

  skip?: number;

  take?: number;
}
