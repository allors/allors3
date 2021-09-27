import { IValidation } from "../derivation/ivalidation";
import { ISession } from "../isession";

export interface ISessionServices {
  onInit(session: ISession): void;

  derive(): IValidation;
}
