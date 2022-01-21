import { Select } from '../../data/select';

export interface FlatResult {
  selectRef?: string;

  select?: Select | any;

  include?: Node[] | any;

  name?: string;

  skip?: number;

  take?: number;
}
