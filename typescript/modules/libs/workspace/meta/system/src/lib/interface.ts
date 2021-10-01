import { Composite, CompositeExtension } from './composite';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface InterfaceExtension extends CompositeExtension {}

export interface Interface extends Composite {
  readonly kind: 'Interface';
  subtypes: Set<Composite>;
}
