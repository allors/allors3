import { Select } from '../../data/select';

export interface FlatResult {
  select?: Select | any;

  include?: Node[] | any;

  name?: string;

  skip?: number;

  take?: number;
}
