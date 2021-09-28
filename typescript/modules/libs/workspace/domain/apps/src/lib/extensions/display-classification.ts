import { Organisation } from '../generated';

export function displayClassification(organisation: Organisation): string {
  return organisation.CustomClassifications.map((w) => w.Name).join(', ');
}
