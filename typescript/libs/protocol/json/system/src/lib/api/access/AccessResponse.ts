import { AccessResponseGrant } from "./AccessResponseGrant";
import { AccessResponseRevocation } from "./AccessResponseRevocation";

export interface AccessResponse {
  /** Grants */
  g: AccessResponseGrant[];

  /** Grants */
  r: AccessResponseRevocation[];
}
