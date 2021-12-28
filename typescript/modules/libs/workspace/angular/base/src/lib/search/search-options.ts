import { Composite, RoleType } from '@allors/workspace/meta/system';
import { And, Predicate, Node } from '@allors/workspace/domain/system';

export interface SearchOptions {
  objectType: Composite;
  roleTypes: RoleType[];
  predicates?: Predicate[];
  post?: (and: And) => void;
  include?: Node[] | any;
}
