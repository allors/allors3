import { AllorsComponent } from '../component';

export abstract class Field extends AllorsComponent {
  protected static counter = 0;

  override dataAllorsKind = 'field';
}
