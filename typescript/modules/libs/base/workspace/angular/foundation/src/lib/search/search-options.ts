import { Composite, RoleType } from '@allors/system/workspace/meta';
import { And, Predicate, Node } from '@allors/system/workspace/domain';

export interface SearchOptions {
  objectType: Composite;
  roleTypes: RoleType[];
  predicates?: Predicate[];
  post?: (and: And) => void;
  include?: Node[] | any;
  take?: number;
}
