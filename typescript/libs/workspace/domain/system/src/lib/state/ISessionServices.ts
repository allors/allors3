import { ISession } from '../ISession';

export interface ISessionServices {
  onInit(session: ISession): void;
}
