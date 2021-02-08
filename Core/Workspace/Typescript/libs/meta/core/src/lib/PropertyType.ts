import { AssociationType } from './AssociationType';
import { OperandType } from './OperandType';
import { RelationType } from './RelationType';

export interface PropertyType extends OperandType {
    relationType: RelationType;
}
