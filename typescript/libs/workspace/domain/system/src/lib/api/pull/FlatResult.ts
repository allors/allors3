import { Select } from "../../data/Select";

export interface FlatResult {
  selectRef?: string;

  select?: Select | any;

  include?: Node[] | any;

  name?: string;

  skip?: number;

  take?: number;
}
